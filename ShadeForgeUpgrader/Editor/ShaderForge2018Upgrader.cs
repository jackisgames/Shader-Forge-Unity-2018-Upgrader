using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ShadeForgeUpgrader
{
    public class ShaderForge2018Upgrader : EditorWindow {
        [MenuItem("Window/Shader Forge/Upgrade To 2018")]
        private static void ShowWindow()
        {
            GetWindow<ShaderForge2018Upgrader>().Init();
        }

        public void Init()
        {
            titleContent=new GUIContent("Shader Forge Upgrade");
            Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("1. Back up your project");
            GUILayout.Label("2. Delete shader forge folder");
            GUILayout.Label("3. Install render pipeline or shader graph");
            GUILayout.Label("4. Click button below!");

            if (GUILayout.Button("Upgrade my shaders!"))
            {
                //find custom pair text
                string[] pairCodeGuid = AssetDatabase.FindAssets("UpgradeKeywords");
                List<Keyword> keywords=new List<Keyword>();

                for (int i = 0; i < pairCodeGuid.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(pairCodeGuid[i]);
                    string[] pairs = File.ReadAllLines(path);

                    for (int j = 0; j < pairs.Length; j += 2)
                    {
                        if (j < pairs.Length - 1)//ignore if there's no pair
                        {
                            keywords.Add(new Keyword()
                            {
                                Old = pairs[j],
                                New = pairs[j + 1]
                            });
                            
                        }
                        if (EditorUtility.DisplayCancelableProgressBar("parsing keywords", path, (float)i/ pairCodeGuid.Length + (float)j / (pairs.Length * pairCodeGuid.Length) ))
                        {
                            return;
                        }
                    }
                }

                string[] assetsGuids = AssetDatabase.FindAssets("t:Shader");
                for (int i = 0; i < assetsGuids.Length; i++)
                {
                
                    string path = AssetDatabase.GUIDToAssetPath(assetsGuids[i]);
                    string[] shaderSource = File.ReadAllLines(path);
                    if (shaderSource[0].Contains("Shader created with Shader Forge"))
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int j = 1; j < shaderSource.Length; j++)
                        {
                            stringBuilder.AppendLine(UpgradeShaderCodePerLine(shaderSource[j], keywords));

                            if (EditorUtility.DisplayCancelableProgressBar("Upgrading shaders", path, (float) i / assetsGuids.Length + (float) j / (shaderSource.Length * assetsGuids.Length)))
                            {
                                return;
                            }
                        }
                        File.WriteAllText(path, stringBuilder.ToString());
                        AssetImporter.GetAtPath(path).SaveAndReimport();
                    }
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private string UpgradeShaderCodePerLine(string code, List<Keyword> keywords)
        {
            for (int i = 0; i < keywords.Count; i++)
            {
                Keyword keyword = keywords[i];
                if (code.Contains(keyword.Old))
                {
                    return keyword.New;
                }
            }
            return code;
        }
    }

    struct Keyword
    {
        public string Old;
        public string New;
    }
}

