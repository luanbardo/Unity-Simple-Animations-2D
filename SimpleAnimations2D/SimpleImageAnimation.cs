using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SimpleAnimations2D
{
    [RequireComponent(typeof(Image))]
    public class SimpleImageAnimation : MonoBehaviour
    {
        private Image image;
        private int currentFrame;
        private float frameTimer;
        private bool isPlaying;

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

        /// <summary>
        /// Event invoked when the animation finishes.
        /// </summary>
        /// <remarks>
        /// Does not trigger for looping animations.
        /// </remarks>>
        public event Action OnAnimationFinished;

        public Sprite[] Frames => frames;

        public float FrameRate => frameRate;

        public bool Loop => loop;

        private void Awake()
        {
            image = GetComponent<Image>();

            if (frames == null || frames.Length == 0)
            {
                enabled = false;
                Debug.LogWarning($"{name}: SimpleUIImageAnimation has no frames!");
                return;
            }

            if (randomStartFrame)
            {
                currentFrame = Random.Range(0, frames.Length);
            }

            image.sprite = frames[currentFrame];
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Restart();
            }
        }

        private void Update()
        {
            if (!isPlaying || frameRate <= 0f)
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

            image.sprite = frames[currentFrame];
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
        /// Stops playing the animation.
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            SetCurrentFrame(0);
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Restart the animation from a random frame if <see cref="randomStartFrame"/> is true, or from the first frame if false.
        /// </summary>
        /// <remarks>
        /// This will set the current frame of the animation and start playing from that frame.
        /// </remarks>
        public void Restart()
        {
            if (randomStartFrame)
            {
                currentFrame = Random.Range(0, frames.Length);
            }
            else
            {
                currentFrame = 0;
            }

            image.sprite = frames[currentFrame];
            Play();
        }


        /// <summary>
        /// Set the current frame of the animation to the specified index.
        /// </summary>
        /// <param name="index">The index of the frame to set.</param>
        public void SetCurrentFrame(int index)
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            currentFrame = Mathf.Clamp(index, 0, frames.Length - 1);
            image.sprite = frames[currentFrame];
        }
    }
}