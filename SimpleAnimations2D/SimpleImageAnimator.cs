using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleAnimations2D
{
    [RequireComponent(typeof(Image))]
    public class SimpleImageAnimator : MonoBehaviour
    {
        private readonly Dictionary<string, SimpleAnimatorClip> clipDictionary = new();
        private Image image;
        private SimpleAnimatorClip currentClip;
        private SimpleAnimatorClip lastPlayedNonOneShotClip;
        private float frameTimer;
        private float currentClipFrameDuration;
        private float speed = 1;
        private bool isCurrentClipOneShot;

        #region Inspector

        [Header("Animations")]
        [SerializeField]
        [Tooltip("List of Simple Animator Clips that this animator can play. " +
                 "First animation will play until other animation is called.")]
        private SimpleAnimatorClip[] clips = Array.Empty<SimpleAnimatorClip>();

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Should the animation start as soon as the object is enabled?")]
        private bool playOnEnable = true;

        [SerializeField]
        [Tooltip("Should the animation use unscaled time? (Recommended for UI)")]
        private bool useUnscaledTime;

        #endregion

        #region Properties

        /// <summary>
        /// Normalized speed for the all animations played by this animator. Cannot be less than 0.01.
        /// </summary>
        public float Speed
        {
            get => speed;
            set => speed = Mathf.Clamp(value, 0.01f, float.MaxValue);
        }

        /// <summary>
        /// (Read-only) Animation playing state.
        /// Use <see cref="Play"/>>, <see cref="Pause"/> & <see cref="Stop"/> to control the animation state.
        /// </summary>
        public int CurrentFrameIndex { get; private set; }

        /// <summary>
        /// (Read-only) Animator playing state.
        /// Use <see cref="Play"/>>, <see cref="Pause"/> & <see cref="Stop"/> to control the animator state.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// (Read-only) Current clip that is being played by the animator.
        /// </summary>
        public SimpleAnimatorClip CurrentClip => currentClip;

        /// <summary>
        /// (Read-only) Is the animator playing a one-shot animation?.
        /// </summary>
        public bool IsPlayingOneShot => isCurrentClipOneShot && IsPlaying;

        public SimpleAnimatorClip[] Clips => clips;

        #endregion

        #region Events
        /// <summary>
        /// Event triggered when an animation finishes.
        /// </summary>
        /// <remarks>
        /// Does not trigger for looping animations.
        /// </remarks>
        public event Action<SimpleAnimatorClip> OnAnimationFinished = _ => { };
        
        /// <summary>
        /// Event triggered when an animation starts.
        /// </summary>
        public event Action<SimpleAnimatorClip> OnAnimationStarted = _ => { };
        
        /// <summary>
        /// Event triggered every time a new frame of an animation starts.
        /// </summary>
        public event Action<SimpleAnimatorClip, int> OnAnimationFrameStarted = (_, _) => { };

        #endregion

        #region Unity Event Functions

        private void Awake()
        {
            image = GetComponent<Image>();

            //Populating clip dictionary with all clips
            for (int i = 0; i < clips.Length; i++)
            {
                clipDictionary.Add(clips[i].ClipName, clips[i]);
            }

            SetCurrentClip(clips[0], false);
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayClip(clips[0], false);
            }
        }

        private void Update()
        {
            if (!IsPlaying || !currentClip || currentClip.FrameRate <= 0f)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            frameTimer += deltaTime * speed;

            if (frameTimer >= currentClipFrameDuration)
            {
                AdvanceFrame();
            }
        }

        #endregion

        #region Private Functions

        private void AdvanceFrame()
        {
            CurrentFrameIndex++;
            frameTimer -= currentClipFrameDuration;

            if (CurrentFrameIndex >= currentClip.Frames.Length)
            {
                if (currentClip.Loop && !isCurrentClipOneShot)
                {
                    CurrentFrameIndex = 0;
                }
                else
                {
                    CurrentFrameIndex = currentClip.Frames.Length - 1;
                    IsPlaying = false;
                    OnAnimationFinished.Invoke(currentClip);

                    if (isCurrentClipOneShot)
                    {
                        PlayClip(lastPlayedNonOneShotClip, false);
                    }

                    return;
                }
            }

            image.sprite = currentClip.Frames[CurrentFrameIndex];
            OnAnimationFrameStarted.Invoke(currentClip, CurrentFrameIndex);
        }

        private void SetCurrentClip(SimpleAnimatorClip clip, bool isOneShot)
        {
            if (clip == null || clip.Frames == null || clip.Frames.Length == 0)
            {
                Debug.LogWarning($"{name}: Trying to set empty or invalid clip!");
                return;
            }

            if (!isOneShot)
            {
                lastPlayedNonOneShotClip = clip;
            }

            isCurrentClipOneShot = isOneShot;
            currentClip = clip;
            CurrentFrameIndex = 0;
            image.sprite = currentClip.Frames[CurrentFrameIndex];
            frameTimer = 0f;
            currentClipFrameDuration = 1 / currentClip.FrameRate;
            OnAnimationStarted.Invoke(currentClip);
            OnAnimationFrameStarted.Invoke(currentClip, CurrentFrameIndex);
        }

        private void PlayClip(SimpleAnimatorClip clip, bool isOneShot)
        {
            SetCurrentClip(clip, isOneShot);
            Play();
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Play the specified animation clip.
        /// </summary>
        /// <param name="clipName">The name of the animation clip to play.</param>
        /// <param name="isOneShot">Should the clip be played as a one shot?</param>
        /// <param name="restartIfAlreadyPlaying">Should the clip restart if it's already playing?</param>
        public void PlayClipByName(string clipName, bool isOneShot, bool restartIfAlreadyPlaying)
        {
            if (currentClip.ClipName == clipName && !restartIfAlreadyPlaying)
            {
                return;
            }

            if (!clipDictionary.TryGetValue(clipName, out SimpleAnimatorClip clip))
            {
                Debug.LogWarning($"{gameObject.name}: Animation {clipName} not found!");
                return;
            }

            PlayClip(clip, isOneShot);
        }

        /// <summary>
        /// Starts playing the current animation.
        /// </summary>
        public void Play() => IsPlaying = true;

        /// <summary>
        /// Pauses the current animation.
        /// </summary>
        public void Pause() => IsPlaying = false;

        /// <summary>
        /// Stops playing the current animation and resets the animator to the default clip.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            SetCurrentClip(clips[0], false);
        }

        #endregion
    }
}