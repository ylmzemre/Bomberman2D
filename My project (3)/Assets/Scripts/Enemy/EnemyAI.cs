using UnityEngine;
using System.Collections.Generic;
using Bomberman2D.Core;

namespace Bomberman2D.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
        public float speed = 2f;
        public LayerMask obstacleMask;
        
        private Rigidbody2D rb;
        private Vector2 moveDirection;
        private bool isMovingToCenter = false;
        private Vector2 targetCenter;
        private Mechanics.SpriteAnimator animator;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Mechanics.SpriteAnimator>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            SnapToGridAndPickDirection();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemy();
            }
        }

        private void FixedUpdate()
        {
            if (isMovingToCenter)
            {
                // Karenin merkezine tam olarak hizalanana kadar yürü
                Vector2 pos = rb.position;
                Vector2 newPos = Vector2.MoveTowards(pos, targetCenter, speed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);

                if (Vector2.Distance(newPos, targetCenter) < 0.01f)
                {
                    isMovingToCenter = false;
                    PickOpenDirection();
                }
            }
            else
            {
                // Mevcut yönde yürümeye devam et
                rb.linearVelocity = moveDirection * speed;
                
                // Önümüzde duvar veya bomba var mı kontrol et (Raycast)
                // 0.55f mesafe, merkezden duvarın dış yüzeyine kadar olan min mesafedir
                RaycastHit2D hit = Physics2D.Raycast(rb.position, moveDirection, 0.55f, obstacleMask);
                if (hit.collider != null)
                {
                    // Duvar algılandığında dur ve grid merkezine dönüp yeni yön ara
                    rb.linearVelocity = Vector2.zero;
                    targetCenter = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
                    isMovingToCenter = true;
                }
            }

            if (animator != null)
            {
                animator.SetDirection(moveDirection);
            }
        }

        private void PickOpenDirection()
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            List<Vector2> openDirections = new List<Vector2>();

            foreach (var dir in directions)
            {
                // Gidilecek yönde engel var mı diye 0.6 birim tarama yap
                if (!Physics2D.Raycast(targetCenter, dir, 0.6f, obstacleMask))
                {
                    openDirections.Add(dir);
                }
            }

            if (openDirections.Count > 0)
            {
                // Çıkmaz sokak değilse, geldiği yöne (geriye) dönmesini engelle
                List<Vector2> forwardDirections = new List<Vector2>(openDirections);
                forwardDirections.Remove(-moveDirection);
                
                if (forwardDirections.Count > 0)
                {
                    moveDirection = forwardDirections[Random.Range(0, forwardDirections.Count)];
                }
                else
                {
                    // Eğer sadece geri dönme şansı varsa (çıkmaz sokak) mecburen geri döner
                    moveDirection = openDirections[Random.Range(0, openDirections.Count)];
                }
            }
            else
            {
                // Her yer kapalıysa bekle
                moveDirection = Vector2.zero;
            }
        }

        private void SnapToGridAndPickDirection()
        {
            targetCenter = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
            rb.position = targetCenter;
            isMovingToCenter = true; // Merkezde değilse merkeze gitsin
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Oyuncuya değerse hasar ver
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player touched enemy!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerDied();
                }
                Destroy(collision.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null && gameObject.scene.isLoaded)
            {
                GameManager.Instance.UnregisterEnemy();
            }
        }
    }
}
