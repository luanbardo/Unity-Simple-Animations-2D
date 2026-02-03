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
        private float frameDuration;
        private float frameTimer;

        #region Inspector

        [Header("Animation")]
        [SerializeField]
        [Tooltip("List of sprites to be used in the animation.")]
        private Sprite[] frames = Array.Empty<Sprite>();

        [SerializeField]
        [Tooltip("How many frames per second should be shown.")]
        private float frameRate = 12f;

        [SerializeField]
        [Tooltip("Should the animation loop indefinitely?")]
        private bool loop = true;
        
        [SerializeField]
        [Tooltip("Should the animation start from a randomized frame?")]
        private bool randomStartFrame;

        [SerializeField]
        [Tooltip("Should the animation start as soon as the object is enabled?")]
        private bool playOnEnable = true;

        [SerializeField]
        [Tooltip("Should the animation use unscaled time? (Recommended for UI)")]
        private bool useUnscaledTime = true;

        #endregion

        #region Properties

        /// <summary>
        /// Normalized speed for the animation. Supports negative values for backwards playback.
        /// </summary>
        public float AnimationSpeed { get; set; } = 1;
        
        /// <summary>
        /// (Read-only) Animation playing state.
        /// Use <see cref="Play"/>>, <see cref="Pause"/> & <see cref="Stop"/> to control the animation state.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Sprite array used for the animation frames.
        /// </summary>
        public Sprite[] Frames
        {
            get => frames;
            set => frames = value;
        }

        /// <summary>
        /// Frame rate used by the animation. Cannot be negative or less than 0.01.
        /// </summary>
        public float FrameRate
        {
            get => frameRate;
            set
            {
                frameRate = Mathf.Clamp(value, 0.01f, float.MaxValue);
                frameDuration = 1 / frameRate;
            }
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the animation finishes.
        /// </summary>
        /// <remarks>
        /// Does not trigger for looping animations.
        /// </remarks>
        public event Action OnAnimationFinished = () => { };

        #endregion

        #region Unity Event Functions

        private void Awake()
        {
            image = GetComponent<Image>();

            if (frames == null || frames.Length == 0)
            {
                enabled = false;
                Debug.LogWarning($"{name}: SimpleAnimation has no frames!");
                return;
            }

            if (randomStartFrame)
            {
                currentFrame = Random.Range(0, frames.Length);
            }

            image.sprite = frames[currentFrame];
            frameDuration = 1 / frameRate;
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
            if (!IsPlaying || frameRate <= 0f)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            frameTimer += deltaTime * MathF.Abs(AnimationSpeed);

            if (frameTimer < frameDuration) return;

            if (AnimationSpeed > 0)
            {
                AdvanceFrame();
            }
            else
            {
                RewindFrame();
            }
        }
        
        private void OnValidate()
        {
            frameRate = Mathf.Clamp(frameRate, 0.01f, float.MaxValue);
        }

        #endregion

        #region Private Functions

        private void AdvanceFrame()
        {
            currentFrame++;
            frameTimer -= frameDuration;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    IsPlaying = false;
                    OnAnimationFinished.Invoke();
                    return;
                }
            }

            image.sprite = frames[currentFrame];
        }

        private void RewindFrame()
        {
            currentFrame--;
            frameTimer -= frameDuration;

            if (currentFrame < 0)
            {
                if (loop)
                {
                    currentFrame = frames.Length - 1;
                }
                else
                {
                    currentFrame = 0;
                    IsPlaying = false;
                    OnAnimationFinished.Invoke();
                    return;
                }
            }

            image.sprite = frames[currentFrame];
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Start playing the animation from the current frame.
        /// </summary>
        public void Play() => IsPlaying = true;

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause() => IsPlaying = false;

        /// <summary>
        /// Stops playing the animation and resets it to the first frame.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            currentFrame = 0;
            frameTimer = 0;
            image.sprite = frames[currentFrame];
        }

        /// <summary>
        /// Restart the animation from the first frame, or from a random frame if <see cref="randomStartFrame"/> is true.
        /// </summary>
        public void Restart()
        {
            currentFrame = randomStartFrame ? Random.Range(0, frames.Length) : 0;
            image.sprite = frames[currentFrame];
            frameTimer = 0;
            Play();
        }

        #endregion
    }
}