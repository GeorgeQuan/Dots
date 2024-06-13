using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorTool
{
    [MenuItem("CustomTool/MergeSprite")]
    public static void MergeSprite()
    {
        string[] sprteGUIDs = Selection.assetGUIDs;//选中的资源
        if (sprteGUIDs == null || sprteGUIDs.Length <= 1) return;//判断是否为空
        List<string> spritePathList = new List<string>(sprteGUIDs.Length);
        for (int i = 0; i < sprteGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(sprteGUIDs[i]);//获取资源路径
            spritePathList.Add(assetPath);
        }
        spritePathList.Sort();//排序

        Texture2D firstTex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[0]);//随便加载一张纹理
        int unitHieght = firstTex.height;//获取宽高
        int unitWidth = firstTex.width;

        Texture2D outputTex = new Texture2D(unitWidth * spritePathList.Count, unitHieght);//定义新纹理的大小
        for (int i = 0; i < spritePathList.Count; i++)//遍历
        {
            Texture2D temp = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[i]);//获取资源
            Color[] colors = temp.GetPixels();//获取像素
            outputTex.SetPixels(i * unitWidth, 0, unitWidth, unitHieght, colors);//根据位置添加像素
        }

        byte[] bytes = outputTex.EncodeToPNG();//把纹理转换成PNG格式
        File.WriteAllBytes(spritePathList[0].Remove(spritePathList[0].LastIndexOf(firstTex.name)) + "MergeSprite.png", bytes);//写入文件,后面是获取路径,并去除原来名字,添加新名字
        AssetDatabase.SaveAssets();//保存资源
        AssetDatabase.Refresh();//刷新资源列表
    }
}
