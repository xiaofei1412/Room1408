using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class Tool_PrefabDataExporter : EditorWindow
{
    [MenuItem("Tools/1408 资产数据导出 (Export Prefab Data)")]
    public static void ExportData()
    {
        // 1. 获取当前在 Hierarchy 中选中的物体
        GameObject selectedObj = Selection.activeGameObject;

        if (selectedObj == null)
        {
            Debug.LogError("物理错误：未选中任何物体！请先在 Hierarchy 中选中 RoomBellisDeluxe 根节点。");
            return;
        }

        // 2. 初始化字符串构建器
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"===== 资产根节点: {selectedObj.name} 数据报告 =====");
        sb.AppendLine("格式说明: [层级] 物体名称 | 局部坐标(Pos) | 网格大小(Bounds) | 材质名称(Material)\n");

        // 3. 递归遍历所有子节点
        TraverseChildren(selectedObj.transform, 0, sb);

        // 4. 将数据写入物理文件
        string exportPath = Application.dataPath + "/PrefabData_Export.txt";
        File.WriteAllText(exportPath, sb.ToString());

        // 5. 刷新编辑器并提示
        AssetDatabase.Refresh();
        Debug.Log($"✅ 数据导出成功！文件已保存至: {exportPath}");
    }

    private static void TraverseChildren(Transform parent, int level, StringBuilder sb)
    {
        foreach (Transform child in parent)
        {
            // 提取缩进层级
            string indent = new string('-', level * 2);
            
            // 提取基础名称与坐标
            string name = child.name;
            Vector3 pos = child.localPosition;
            string posStr = $"({pos.x:F2}, {pos.y:F2}, {pos.z:F2})";

            // 提取网格包围盒大小（帮助判断它是大墙壁还是小抽屉）
            string boundsStr = "无网格";
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Vector3 size = mf.sharedMesh.bounds.size;
                boundsStr = $"体积: {size.x:F2} x {size.y:F2} x {size.z:F2}";
            }

            // 提取材质名称（帮助判断它是木头、玻璃还是镜子）
            string matStr = "无材质";
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null)
            {
                matStr = mr.sharedMaterial.name;
            }

            // 拼接单行数据
            sb.AppendLine($"{indent}> {name} | Pos:{posStr} | {boundsStr} | 材质:{matStr}");

            // 递归读取下一层
            TraverseChildren(child, level + 1, sb);
        }
    }
}