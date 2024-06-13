using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorTool
{
    [MenuItem("CustomTool/MergeSprite")]
    public static void MergeSprite()
    {
        string[] sprteGUIDs = Selection.assetGUIDs;//ѡ�е���Դ
        if (sprteGUIDs == null || sprteGUIDs.Length <= 1) return;//�ж��Ƿ�Ϊ��
        List<string> spritePathList = new List<string>(sprteGUIDs.Length);
        for (int i = 0; i < sprteGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(sprteGUIDs[i]);//��ȡ��Դ·��
            spritePathList.Add(assetPath);
        }
        spritePathList.Sort();//����

        Texture2D firstTex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[0]);//������һ������
        int unitHieght = firstTex.height;//��ȡ���
        int unitWidth = firstTex.width;

        Texture2D outputTex = new Texture2D(unitWidth * spritePathList.Count, unitHieght);//����������Ĵ�С
        for (int i = 0; i < spritePathList.Count; i++)//����
        {
            Texture2D temp = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[i]);//��ȡ��Դ
            Color[] colors = temp.GetPixels();//��ȡ����
            outputTex.SetPixels(i * unitWidth, 0, unitWidth, unitHieght, colors);//����λ���������
        }

        byte[] bytes = outputTex.EncodeToPNG();//������ת����PNG��ʽ
        File.WriteAllBytes(spritePathList[0].Remove(spritePathList[0].LastIndexOf(firstTex.name)) + "MergeSprite.png", bytes);//д���ļ�,�����ǻ�ȡ·��,��ȥ��ԭ������,���������
        AssetDatabase.SaveAssets();//������Դ
        AssetDatabase.Refresh();//ˢ����Դ�б�
    }
}
