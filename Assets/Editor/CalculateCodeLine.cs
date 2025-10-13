using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

public class CalculateCodeLine
{
    static string calcPath = Application.dataPath; // 统计整个项目的代码行数（默认为 Assets 文件夹）

    [MenuItem("Tools/CalculateCodeLine")]
    static void CalcCode()
    {
        if (!Directory.Exists(calcPath))
        {
            Debug.LogError(string.Format("Path Not Exist  : \"{0}\" ", calcPath));
            return;
        }

        // 支持的文件类型
        string[] fileExtensions = new[] { "*.cs", "*.js", "*.shader", "*.txt" };
        string[] allFiles = new string[0];

        // 搜索功能，支持多种文件类型
        foreach (string ext in fileExtensions)
        {
            string[] files = Directory.GetFiles(calcPath, ext, SearchOption.AllDirectories);
            allFiles = System.Linq.Enumerable.Concat(allFiles, files).ToArray();
        }

        int totalLine = 0; // 总行数
        int emptyLines = 0; // 空行数
        int commentLines = 0; // 注释行数

        foreach (var file in allFiles)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    totalLine++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        emptyLines++;
                    }
                    else if (IsCommentLine(line))
                    {
                        commentLines++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error reading file: {0} - {1}", file, e.Message));
            }
        }

        int codeLines = totalLine - emptyLines - commentLines; // 有效代码行数
        Debug.Log(string.Format("统计结果:\n" +
            "总行数: {0}\n" +
            "有效代码行: {1}\n" +
            "空行: {2}\n" +
            "注释行: {3}\n" +
            "文件数: {4}",
            totalLine,
            codeLines,
            emptyLines,
            commentLines,
            allFiles.Length));
    }

    static bool IsCommentLine(string line)
    {
        // 检查是否是注释行
        string trimmedLine = line.Trim();
        return trimmedLine.StartsWith("//") ||
               trimmedLine.StartsWith("/*") ||
               trimmedLine.EndsWith("*/") ||
               (trimmedLine.Contains("/*") && trimmedLine.Contains("*/"));
    }
}