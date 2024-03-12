using System.Collections.Generic;
using System.IO; 
using UnityEditor;
using UnityEngine;

public static class EditorTool
{
     [MenuItem("Assets/CustomTool/MergeSprite")]
     public static void MergeSprite()
     {
          string[] sprteGUIs = Selection.assetGUIDs;
          if (sprteGUIs == null || sprteGUIs.Length <= 1)
          {
               return;
          }

          List<string> spritePathList = new List<string>();

          for (int i = 0; i < sprteGUIs.Length; i++)
          {
               string assetPath = AssetDatabase.GUIDToAssetPath(sprteGUIs[i]);
               spritePathList.Add(assetPath);
          }
          
          spritePathList.Sort();
          Texture2D firstTex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[0]);
          int height = firstTex.height;
          int width = firstTex.width;

          Texture2D  outputTex = new Texture2D(width * spritePathList.Count,height);
          for (int i = 0; i < spritePathList.Count; i++)
          {
               Texture2D tmp = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[i]);
               Color[] colors = tmp.GetPixels();
               outputTex.SetPixels(i * width,0,width,height,colors);
          }

          byte[] bytes = outputTex.EncodeToPNG();
          File.WriteAllBytes($"{spritePathList[0].Remove(spritePathList[0].LastIndexOf(firstTex.name))}MergeSprite.png",bytes);
          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh();
     }
}
