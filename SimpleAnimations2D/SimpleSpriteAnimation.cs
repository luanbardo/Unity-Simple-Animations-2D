using System;
using UnityEngine;

namespace SimpleAnimations2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleSpriteAnimation : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private int currentFrame;
        private float frameTimer;
        private bool isPlaying;
        private bool isVisible = true;

        [Header("Animation")]
        [SerializeField]
        private Sprite[] frames;

        [SerializeField]
        private float frameRate = 12f;

        [SerializeField]
        private bool loop = true;

        [SerializeField]
        private bool playOnEnable = true;

        [SerializeField]
        private bool randomStartFrame;

        [Header("Performance")]
        [SerializeField]
        public bool playWhenNotVisible = true;
        
        public Sprite[] Frames => frames;
        public float FrameRate => frameRate;
        public bool Loop => loop;

        /// <summary>
        /// Event invoked when the animation finishes.
        /// </summary>
        /// <remarks>
        /// Does not trigger for looping animations.
        /// </remarks>>
        public event Action OnAnimationFinished;


        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (frames == null || frames.Length == 0)
            {
                enabled = false;
                Debug.LogWarning($"{name}: SimpleAnimation has no frames!");
                return;
            }

            if (randomStartFrame)
            {
                currentFrame = UnityEngine.Random.Range(0, frames.Length);
            }

            spriteRenderer.sprite = frames[currentFrame];
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Restart();
            }
        }
        
        private void OnBecameInvisible()
        {
            isVisible = false;
        }

        private void OnBecameVisible()
        {
            isVisible = true;
        }

        private void Update()
        {
            if (!isPlaying || frameRate <= 0f)
            {
                return;
            }

            if (!playWhenNotVisible && !isVisible)
            {
                return;
            }

            frameTimer += Time.deltaTime;

            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    isPlaying = false;
                    OnAnimationFinished?.Invoke();
                    return;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
        }

        /// <summary>
        /// Start playing the animation from the current frame.
        /// </summary>
        public void Play()
        {
            isPlaying = true;
            frameTimer = 0f;
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
        }
        
        /// <summary>
        /// Stops playing the animation.
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            SetCurrentFrameIndex(0);
        }

        /// <summary>
        /// Restart the animation from the first frame, or from a random frame if <see cref="randomStartFrame"/> is true.
        /// </summary>
        public void Restart()
        {
            if (randomStartFrame)
            {
                currentFrame = UnityEngine.Random.Range(0, frames.Length);
            }
            else
            {
                currentFrame = 0;
            }

            spriteRenderer.sprite = frames[currentFrame];
            Play();
        }

        /// <summary>
        /// Sets the sprite frames to be used by the animation.
        /// </summary>
        /// <param name="frames">The sprite frames to be used by the animation.</param>
        public void SetSpriteFrames(Sprite[] frames)
        {
            this.frames = frames;
        }

        /// <summary>
        /// Sets the current frame index of the animation to the specified index.
        /// If the specified index is out of range, it will be clamped to the nearest valid index.
        /// </summary>
        /// <param name="index">The index of the frame to set.</param>
        public void SetCurrentFrameIndex(int index)
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            currentFrame = Mathf.Clamp(index, 0, frames.Length - 1);
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}