using UnityEngine;

namespace Bomberman2D.Mechanics
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] upSprites;
        public Sprite[] downSprites;
        public Sprite[] leftSprites;
        public Sprite[] rightSprites;
        
        public bool useFlipForLeft = false; 
        public float frameRate = 0.15f;
        
        private SpriteRenderer sr;
        private float timer;
        private int currentFrame;
        private Vector2 currentDirection = Vector2.down;
        private bool isMoving = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                isMoving = true;
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    currentDirection = direction.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    currentDirection = direction.y > 0 ? Vector2.up : Vector2.down;
                }
            }
            else
            {
                if (isMoving) // Sadece yeni durduğunda frame'i sıfırla
                {
                    isMoving = false;
                    currentFrame = 0; 
                    UpdateSprite();
                }
            }
        }

        private void Update()
        {
            if (!isMoving) return;

            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer -= frameRate;
                currentFrame++;
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            if (sr == null) return;

            Sprite[] currentArray = GetCurrentArray();
            if (currentArray == null || currentArray.Length == 0) return;
            
            if (currentFrame >= currentArray.Length)
            {
                currentFrame = 0;
            }
            
            sr.sprite = currentArray[currentFrame];
        }

        private Sprite[] GetCurrentArray()
        {
            if (currentDirection == Vector2.left && useFlipForLeft)
            {
                sr.flipX = true;
                return rightSprites;
            }
            
            sr.flipX = false;
            
            if (currentDirection == Vector2.up) return upSprites;
            if (currentDirection == Vector2.down) return downSprites;
            if (currentDirection == Vector2.left) return leftSprites;
            if (currentDirection == Vector2.right) return rightSprites;
            
            return downSprites;
        }
    }
}
