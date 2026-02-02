using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleAnimations2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleSpriteAnimator : MonoBehaviour
    {
        private readonly Dictionary<string, SimpleAnimatorClip> clipDictionary = new();
        private SpriteRenderer spriteRenderer;
        private SimpleAnimatorClip currentClip;
        private float frameTimer;
        private bool isVisible = true;
        private bool isOneShot;

        [Header("Animations")]
        [SerializeField]
        private SimpleAnimatorClip defaultClip;
        
        [SerializeField]
        private SimpleAnimatorClip[] clips;

        [Header("Settings")]
        [SerializeField]
        private bool playOnEnable = true;

        [Range(0.1f, 5f)]
        [SerializeField]
        private float speed = 1f;

        [Header("Performance")]
        [SerializeField]
        private bool playWhenNotVisible = true;
        
        public event Action<string> OnAnimationFinished;
        public event Action<string> OnOneShotAnimationFinished;
        public event Action<string, int> OnOneShotAnimationFrameStarted;
        
        public int CurrentClipFrame { get; private set; }
        
        public string CurrentClipName { get; private set; }
        
        public bool IsPlaying { get; private set; }

        public bool IsPlayingOneShot => isOneShot && IsPlaying;

        public SimpleAnimatorClip[] Clips => clips;

        public float Speed => speed;


        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (defaultClip == null)
            {
                Debug.LogError($"{name}: SimpleAnimator has no default animation!");
                return;
            }

            if (defaultClip != null)
            {
                SetClip(defaultClip);
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clipDictionary.Add(clips[i].ClipName, clips[i]);
            }

            clipDictionary.TryAdd(defaultClip.ClipName, defaultClip);
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayDefaultClip();
            }
        }
        
        private void OnDisable()
        {
            // If a one shot animation is playing when disabled, trigger the finish event
            if (isOneShot && IsPlaying)
            {
                OnOneShotAnimationFinished?.Invoke(CurrentClipName);
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
            // One shot animations always play regardless of visibility
            if (!isOneShot && !playWhenNotVisible && !isVisible)
            {
                return;
            }

            if (!IsPlaying || !currentClip || currentClip.FrameRate <= 0f)
            {
                return;
            }

            frameTimer += Time.deltaTime * speed;

            if (frameTimer >= 1f / currentClip.FrameRate)
            {
                frameTimer -= 1f / currentClip.FrameRate;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            CurrentClipFrame++;

            if (CurrentClipFrame >= currentClip.Frames.Length)
            {
                if (currentClip.Loop && !isOneShot)
                {
                    CurrentClipFrame = 0;
                }
                else
                {
                    CurrentClipFrame = currentClip.Frames.Length - 1;
                    IsPlaying = false;
                    OnAnimationFinished?.Invoke(currentClip.ClipName);

                    // Trigger one shot finish event and return to default
                    if (isOneShot)
                    {
                        string finishedAnimName = CurrentClipName;
                        OnOneShotAnimationFinished?.Invoke(finishedAnimName);
                        PlayDefaultClip();
                    }

                    return;
                }
            }

            spriteRenderer.sprite = currentClip.Frames[CurrentClipFrame];

            // Trigger frame started event for one shot animations
            if (isOneShot)
            {
                OnOneShotAnimationFrameStarted?.Invoke(CurrentClipName, CurrentClipFrame);
            }
        }

        private void SetClip(SimpleAnimatorClip clip)
        {
            if (clip == null || clip.Frames == null || clip.Frames.Length == 0)
            {
                Debug.LogWarning($"{name}: Trying to set empty or invalid clip!");
                return;
            }

            currentClip = clip;
            CurrentClipName = clip.ClipName;
            CurrentClipFrame = 0;
            spriteRenderer.sprite = currentClip.Frames[CurrentClipFrame];
            frameTimer = 0f;

            // Trigger frame started event for one shot animations (for initial frame)
            if (isOneShot)
            {
                OnOneShotAnimationFrameStarted?.Invoke(CurrentClipName, CurrentClipFrame);
            }
        }

        /// <summary>
        /// Set the default animation clip for the animator.
        /// </summary>
        /// <param name="clip">The animation clip to set as the default.</param>
        public void SetDefaultClip(SimpleAnimatorClip clip)
        {
            clipDictionary.Remove(defaultClip.name);
            defaultClip = clip;
            clipDictionary.TryAdd(clip.ClipName, clip);
        }

        /// <summary>
        /// Play the specified animation clip.
        /// </summary>
        /// <param name="name">The name of the animation clip to play.</param>
        /// <remarks>
        /// If the animation is already playing, this method does nothing.
        /// If the animation is not found, a warning is logged to the console.
        /// </remarks>
        public void PlayClip(string name)
        {
            if(CurrentClipName == name)
            {
                return;
            }

            if (!clipDictionary.TryGetValue(name, out SimpleAnimatorClip clip))
            {
                Debug.LogWarning($"{gameObject.name}: Animation {name} not found!");
                return;
            }

            SetClip(clip);
            isOneShot = false;
            Play();
        }

        /// <summary>
        /// Play the specified animation clip once.
        /// </summary>
        /// <param name="name">The name of the animation clip to play.</param>
        /// <remarks>
        /// If the animation is not found, a warning is logged to the console.
        /// </remarks>
        public void PlayClipOneShot(string name)
        {
            if (!clipDictionary.TryGetValue(name, out SimpleAnimatorClip clip))
            {
                Debug.LogWarning($"{gameObject.name}: Animation {name} not found!");
                return;
            }

            SetClip(clip);
            isOneShot = true;
            Play();
        }
       
        /// <summary>
        /// Play the default animation clip.
        /// </summary>
        /// <remarks>
        /// If the default animation clip is not set, or if the current animation clip is already the default animation clip, this method does nothing.
        /// Otherwise, it sets the current animation clip to the default animation clip and starts playing it.
        /// </remarks>
        public void PlayDefaultClip()
        {
            if(CurrentClipName == defaultClip.ClipName)
            {
                return;
            }

            if (defaultClip != null)
            {
                SetClip(defaultClip);
                isOneShot = false;
                Play();
            }
        }

        /// <summary>
        /// Sets the speed of the animation.
        /// </summary>
        /// <param name="newSpeed">The new speed of the animation. The speed is clamped to a minimum of 0.01f to prevent the animation from freezing.</param>
        /// <remarks>
        /// This method is useful for fine-tuning the animation speed in the inspector.
        /// </remarks>
        public void SetSpeed(float newSpeed)
        {
            speed = Mathf.Max(0.01f, newSpeed);
        }

        /// <summary>
        /// Start playing the animation.
        /// </summary>
        /// <remarks>
        /// This method will start playing the animation from the current frame.
        /// If the animation is already playing, this method does nothing.
        /// </remarks>
        public void Play()
        {
            IsPlaying = true;
        }

        /// <summary>
        /// Pause the animation. This will stop the animation from playing
        /// until <see cref="Play"/> is called again.
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Stop playing the animation and reset to the default clip.
        /// </summary>
        /// <remarks>
        /// This method will stop playing the animation and reset the
        /// animator to its default state. The animation will be
        /// stopped, and the current clip will be set to the default clip.
        /// The current frame will also be reset to 0.
        /// </remarks>
        public void Stop()
        {
            IsPlaying = false;
            currentClip = defaultClip;
            CurrentClipName = defaultClip.ClipName;
            CurrentClipFrame = 0;
        }
    }
}