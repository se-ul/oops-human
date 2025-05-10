using UnityEngine;
using UnityEditor;

public class AssignTexturesToSelectedMaterials : EditorWindow
{
    [MenuItem("Tools/Assign Textures to Selected Materials")]
    public static void AssignTextures()
    {
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 머티리얼이 없습니다.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            if (obj is not Material mat)
            {
                Debug.LogWarning($"{obj.name}은(는) 머티리얼이 아닙니다. 건너뜁니다.");
                continue;
            }

            if (!mat.name.StartsWith("M_"))
            {
                Debug.LogWarning($"'{mat.name}' 이름이 'M_'으로 시작하지 않습니다. 건너뜁니다.");
                continue;
            }

            string baseName = mat.name.Substring(2); // "M_" 제거
            string colorTexName = "T_" + baseName + "_C";
            string normalTexName = "T_" + baseName + "_N";

            Texture colorTex = FindTextureByName(colorTexName);
            Texture normalTex = FindTextureByName(normalTexName);

            if (colorTex != null)
            {
                mat.SetTexture("_MainTex", colorTex); // 실제 property 이름에 맞게 조정 필요
                Debug.Log($"[✓] BaseColor '{colorTexName}' → {mat.name}");
            }
            else
            {
                Debug.LogWarning($"[!] BaseColor '{colorTexName}' not found for {mat.name}");
            }

            if (normalTex != null)
            {
                mat.SetTexture("_NormalMap", normalTex); // 실제 property 이름에 맞게 조정 필요
                Debug.Log($"[✓] NormalMap '{normalTexName}' → {mat.name}");
            }
            else
            {
                Debug.LogWarning($"[!] NormalMap '{normalTexName}' not found for {mat.name}");
            }

            EditorUtility.SetDirty(mat);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("🎯 선택한 모든 머티리얼에 텍스처를 적용했습니다.");
    }

    private static Texture FindTextureByName(string texName)
    {
        string[] texGuids = AssetDatabase.FindAssets(texName + " t:Texture");
        foreach (string guid in texGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex != null)
                return tex;
        }
        return null;
    }
}
