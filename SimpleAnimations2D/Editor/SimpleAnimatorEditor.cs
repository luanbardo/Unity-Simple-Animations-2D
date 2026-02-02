using UnityEditor;
using UnityEngine;

namespace SimpleAnimations2D.Editor
{
    [CustomEditor(typeof(SimpleSpriteAnimator))]
    public class SimpleAnimatorEditor : UnityEditor.Editor
    {
        private SimpleSpriteAnimator animator;
        private SpriteRenderer spriteRenderer;
        private Sprite initialSprite;
        private int selectedAnimationIndex = -1;
        private bool isPlaying;
        private bool isPaused;
        private bool previewStarted;
        private bool previewOneShot;
        private bool overrideSpeed;
        private int currentFrame;
        private float frameTimer;
        private double lastEditorTime;
        private float previewSpeed = 1f;

        protected void OnEnable()
        {
            animator = (SimpleSpriteAnimator)target;
            spriteRenderer = animator.GetComponent<SpriteRenderer>();

            EditorApplication.update += EditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected void OnDisable()
        {
            StopPreviewAndRestore();

            EditorApplication.update -= EditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            DrawAnimationDropdown();
            DrawPreviewOptions();
            DrawPreviewControls();
            DrawScrubBar();

            EditorGUILayout.EndVertical();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                StopPreviewAndRestore();
            }
        }

        private void DrawAnimationDropdown()
        {
            SimpleAnimatorClip[] animations = animator.Clips;

            if (animations == null || animations.Length == 0)
            {
                EditorGUILayout.HelpBox("No animations available.", MessageType.Info);
                return;
            }

            string[] options = new string[animations.Length];
            for (int i = 0; i < animations.Length; i++)
                options[i] = animations[i] != null ? animations[i].ClipName : "<null>";

            int newIndex = EditorGUILayout.Popup("Preview Animation", selectedAnimationIndex, options);

            if (newIndex != selectedAnimationIndex)
            {
                StopPreviewAndRestore();
                selectedAnimationIndex = newIndex;
            }
        }

        private void DrawPreviewOptions()
        {
            previewOneShot = EditorGUILayout.Toggle("One Shot", previewOneShot);

            overrideSpeed = EditorGUILayout.Toggle("Override Speed", overrideSpeed);
            if (overrideSpeed)
            {
                previewSpeed = EditorGUILayout.Slider("Preview Speed", previewSpeed, 0.1f, 5f);
            }
        }

        private void DrawPreviewControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = CanPlay();

                if (GUILayout.Button("Play"))
                {
                    PlayPreview();
                }

                if (GUILayout.Button("Pause"))
                {
                    PausePreview();
                }

                if (GUILayout.Button("Stop"))
                {
                    StopPreviewAndRestore();
                }

                GUI.enabled = true;
            }
        }

        private void DrawScrubBar()
        {
            if (!CanPlay())
            {
                return;
            }

            SimpleAnimatorClip clip = animator.Clips[selectedAnimationIndex];
            int maxFrame = clip.Frames.Length - 1;

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            int newFrame = EditorGUILayout.IntSlider("Frame", currentFrame, 0, maxFrame);
            if (EditorGUI.EndChangeCheck())
            {
                BeginPreviewSession();
                isPlaying = true;
                isPaused = true;
                currentFrame = newFrame;
                spriteRenderer.sprite = clip.Frames[currentFrame];
                EditorUtility.SetDirty(spriteRenderer);
            }
        }

        private bool CanPlay()
        {
            if (animator.Clips == null)
            {
                return false;
            }

            if (selectedAnimationIndex < 0 || selectedAnimationIndex >= animator.Clips.Length)
            {
                return false;
            }

            SimpleAnimatorClip clip = animator.Clips[selectedAnimationIndex];
            return clip != null && clip.Frames != null && clip.Frames.Length != 0;
        }

        private void PlayPreview()
        {
            if (isPlaying && !isPaused)
            {
                return;
            }

            BeginPreviewSession();

            if (!isPlaying)
            {
                currentFrame = 0;
                frameTimer = 0f;
            }

            isPlaying = true;
            isPaused = false;
            lastEditorTime = EditorApplication.timeSinceStartup;
        }

        private void PausePreview()
        {
            if (isPlaying)
            {
                isPaused = true;
            }
        }

        private void BeginPreviewSession()
        {
            if (previewStarted)
            {
                return;
            }

            previewStarted = true;
            initialSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        }

        private void StopPreviewAndRestore()
        {
            if (!previewStarted)
            {
                return;
            }

            isPlaying = false;
            isPaused = false;
            previewStarted = false;

            currentFrame = 0;
            frameTimer = 0f;

            if (spriteRenderer != null && initialSprite != null)
            {
                spriteRenderer.sprite = initialSprite;
                EditorUtility.SetDirty(spriteRenderer);
            }

            initialSprite = null;
        }

        private void EditorUpdate()
        {
            if (!isPlaying || isPaused || !CanPlay())
            {
                return;
            }

            SimpleAnimatorClip clip = animator.Clips[selectedAnimationIndex];

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(now - lastEditorTime);
            lastEditorTime = now;

            float speed = overrideSpeed ? previewSpeed : animator.Speed;
            frameTimer += deltaTime * speed;

            float frameDuration = 1f / Mathf.Max(clip.FrameRate, 0.0001f);

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                AdvanceFrame(clip);
            }
        }

        private void AdvanceFrame(SimpleAnimatorClip clip)
        {
            currentFrame++;

            bool loop = clip.Loop && !previewOneShot;

            if (currentFrame >= clip.Frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = clip.Frames.Length - 1;
                    StopPreviewAndRestore();
                    return;
                }
            }

            spriteRenderer.sprite = clip.Frames[currentFrame];
            EditorUtility.SetDirty(spriteRenderer);
        }
    }
}