using System;
using UnityEngine;

namespace SimpleAnimations2D
{
    [CreateAssetMenu(fileName = "SAC_AnimationName", menuName = "Simple Animations/Simple Animation Clip")]
    public class SimpleAnimatorClip : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The name used to play this animation in the animator component. Cannot be empty.")]
        private string clipName = "default";

        [SerializeField]
        [Tooltip("List of sprites to be used in the animation.")]
        private Sprite[] frames;

        [SerializeField]
        [Tooltip("How many frames per second should be shown.")]
        private float frameRate = 12f;

        [SerializeField]
        [Tooltip("Should the animation loop indefinitely?")]
        private bool loop;

        public string ClipName => clipName;

        public Sprite[] Frames => frames;

        public float FrameRate => frameRate;

        public bool Loop => loop;

        public void SetClipFrames(Sprite[] frames)
        {
            this.frames = frames;
        }

        private void OnValidate()
        {
            if (clipName == "")
            {
                Debug.LogWarning($"{name}: Clip Name cannot be empty!");
            }
            
            frameRate = Mathf.Clamp(frameRate, 0.01f, float.MaxValue);
        }
    }
}