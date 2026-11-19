using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CorrigirMateriaisURP
{
    [MenuItem("Blackout51/Corrigir texturas dos materiais selecionados")]
    static void Corrigir()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        int total = 0;

        foreach (Object obj in Selection.objects)
        {
            Material mat = obj as Material;
            if (mat == null) continue;

            // Ler as texturas guardadas no material (mesmo as do shader antigo)
            var texturas = new Dictionary<string, Texture>();
            var so = new SerializedObject(mat);
            var lista = so.FindProperty("m_SavedProperties.m_TexEnvs");
            for (int i = 0; i < lista.arraySize; i++)
            {
                var item = lista.GetArrayElementAtIndex(i);
                string nome = item.FindPropertyRelative("first").stringValue;
                var tex = item.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                if (tex != null) texturas[nome] = tex;
            }

            Texture cor = Procurar(texturas, "_BaseMap", "_MainTex", "_BaseColorMap", "_AlbedoMap", "_Albedo", "_Diffuse");
            Texture normal = Procurar(texturas, "_BumpMap", "_NormalMap", "_Normal");
            Texture metal = Procurar(texturas, "_MetallicGlossMap", "_MetallicMap");
            Texture ao = Procurar(texturas, "_OcclusionMap");

            // Plano B: textura com o mesmo nome do material (ex.: Mi_001_BaseColor)
            if (cor == null)
            {
                string[] guids = AssetDatabase.FindAssets(mat.name + "_BaseColor t:Texture2D");
                if (guids.Length > 0)
                    cor = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            Undo.RecordObject(mat, "Corrigir material URP");
            mat.shader = lit;

            if (cor != null) { mat.SetTexture("_BaseMap", cor); mat.SetColor("_BaseColor", Color.white); }
            if (normal != null) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); }
            if (metal != null) { mat.SetTexture("_MetallicGlossMap", metal); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); }
            if (ao != null) { mat.SetTexture("_OcclusionMap", ao); mat.EnableKeyword("_OCCLUSIONMAP"); }

            EditorUtility.SetDirty(mat);
            Debug.Log($"{mat.name}: cor = {(cor ? cor.name : "NENHUMA")}, normal = {(normal ? normal.name : "-")}, metal = {(metal ? metal.name : "-")}");
            total++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Materiais corrigidos: {total}");
    }

    static Texture Procurar(Dictionary<string, Texture> texturas, params string[] nomes)
    {
        foreach (string n in nomes)
            if (texturas.TryGetValue(n, out Texture t)) return t;
        return null;
    }
}