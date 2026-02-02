using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SimpleAnimations2D.Editor
{
    public class SpritesheetToClipTool : UnityEditor.Editor
    {
        private const string FILE_PREFIX = "SAC_";
        
        [MenuItem("Assets/Simple Animations 2D/Create Simple Animator Clip", true)]
        private static bool ValidateCreateClipFromSpritesheet()
        {
            // Only show menu item if a texture is selected
            Object selected = Selection.activeObject;
            if (selected == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
            // Check if it's a texture with sprite mode set to Multiple
            return importer != null && importer.spriteImportMode == SpriteImportMode.Multiple;
        }

        [MenuItem("Assets/Simple Animations 2D/Create Simple Animator Clip")]
        private static void CreateClipFromSpritesheet()
        {
            Object selected = Selection.activeObject;
            if (selected == null)
            {
                Debug.LogWarning("No spritesheet selected!");
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
        
            // Load all sprites from the spritesheet
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite[] spriteFrames = System.Array.FindAll(sprites, obj => obj is Sprite)
                .Select(obj => obj as Sprite)
                .OrderBy(s =>
                {
                    // Extract the last number in the sprite name
                    Match match = Regex.Match(s.name, @"\d+$");
                    return match.Success ? int.Parse(match.Value) : 0;
                })
                .ToArray();

            if (spriteFrames.Length == 0)
            {
                Debug.LogWarning("No sprites found in the selected texture! Make sure it's sliced in Sprite Editor.");
                return;
            }

            // Create the animation clip
            SimpleAnimatorClip clip = CreateInstance<SimpleAnimatorClip>();
            clip.SetClipFrames(spriteFrames);

            // Save the clip in the same folder as the spritesheet
            string directory = Path.GetDirectoryName(path);
            string fileName = $"{FILE_PREFIX}{Path.GetFileNameWithoutExtension(path)}";
            string clipPath = Path.Combine(directory, fileName + ".asset");
        
            // Make sure the path is unique
            clipPath = AssetDatabase.GenerateUniqueAssetPath(clipPath);
        
            AssetDatabase.CreateAsset(clip, clipPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select and ping the new asset
            EditorGUIUtility.PingObject(clip);
            Selection.activeObject = clip;

            Debug.Log($"Created Simple Animator Clip with {spriteFrames.Length} frames at: {clipPath}");
        }
    }
}