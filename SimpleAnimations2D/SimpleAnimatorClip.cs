using UnityEngine;

namespace SimpleAnimations2D
{
    [CreateAssetMenu(fileName = "SAC_AnimationName", menuName = "Simple Animations/Simple Animation Clip")]
    public class SimpleAnimatorClip : ScriptableObject
    {
        private string clipName = "default";
        private Sprite[] frames;
        private float frameRate = 12f;
        private bool loop = false;

        public string ClipName => clipName;

        public Sprite[] Frames => frames;

        public float FrameRate => frameRate;

        public bool Loop => loop;

        public void SetClipFrames(Sprite[] frames)
        {
            this.frames = frames;
        }
    }
}