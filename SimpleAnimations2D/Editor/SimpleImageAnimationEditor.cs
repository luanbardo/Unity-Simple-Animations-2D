using UnityEditor;
using UnityEngine;

namespace SimpleAnimations2D.Editor
{
    [CustomEditor(typeof(SimpleImageAnimation))]
    public class SimpleImageAnimationEditor : UnityEditor.Editor
    {
        private SimpleImageAnimation animator;
        private SpriteRenderer spriteRenderer;
        private Sprite initialSprite;
        private bool isPlaying;
        private bool isPaused;
        private bool previewStarted;
        private int currentFrame;
        private float frameTimer;
        private double lastEditorTime;


        protected void OnEnable()
        {
            animator = (SimpleImageAnimation)target;
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

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                StopPreviewAndRestore();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            DrawPreviewControls();
            DrawScrubBar();

            EditorGUILayout.EndVertical();
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

            int maxFrame = animator.Frames.Length - 1;

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            int newFrame = EditorGUILayout.IntSlider("Frame", currentFrame, 0, maxFrame);

            if (EditorGUI.EndChangeCheck())
            {
                BeginPreviewSession();
                isPlaying = true;
                isPaused = true;
                currentFrame = newFrame;
                spriteRenderer.sprite = animator.Frames[currentFrame];
                EditorUtility.SetDirty(spriteRenderer);
            }
        }

        private bool CanPlay()
        {
            return animator.Frames != null && animator.Frames.Length != 0;
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
            if (!isPlaying)
            {
                return;
            }

            isPaused = true;
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


            double now = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(now - lastEditorTime);
            lastEditorTime = now;

            float speed = 1;
            frameTimer += deltaTime * speed;

            float frameDuration = 1f / Mathf.Max(animator.FrameRate, 0.0001f);

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            currentFrame++;

            bool loop = animator.Loop;

            if (currentFrame >= animator.Frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = animator.Frames.Length - 1;
                    StopPreviewAndRestore();
                    return;
                }
            }

            spriteRenderer.sprite = animator.Frames[currentFrame];
            EditorUtility.SetDirty(spriteRenderer);
        }
    }
}