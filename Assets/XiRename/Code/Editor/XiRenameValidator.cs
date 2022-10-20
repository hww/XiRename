using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XiRenameTool.Editor
{
    public static class XiRenameValidator
    {
        public static void ValidateSelectetItems()
        {
            // Try to work out what folder we're clicking on. This code is from google.
            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var item = new RenamableObject(obj);

                switch (item.Type)
                {
                    case ERenamableType.Directory:
                        ValidateItemsInFolder(item.OriginalPath);
                        break;
                    case ERenamableType.File:
                        ValidateItem(item);
                        break;
                    case ERenamableType.GameObject:
                        break;
                }
            }


        }
        public static void ValidateItemsInFolder(string folderPath)
        {
            var objects = GetAssetList<UnityEngine.Object>(folderPath);
            foreach (var obj in objects)
            {
                var item = new RenamableObject(obj);
                switch (item.Type)
                {
                    case ERenamableType.Directory:
                        ValidateItemsInFolder(item.OriginalPath);
                        break;
                    case ERenamableType.File:
                        ValidateItem(item);
                        break;
                    case ERenamableType.GameObject:
                        break;
                }
            }

        }

        public static void ValidateItem(RenamableObject item)
        {
            XiRename.AutoValidateName(item);
            switch (item.State)
            {
                case EFileState.Ignored:
                    break;
                case EFileState.Undefined:
                    Debug.LogWarning($"[Undefined] {item.FileName}", item.Reference);
                    break;
                case EFileState.Invalid:
                    Debug.LogError($"[Invalid] {item.FileName}", item.Reference);
                    break;
                case EFileState.Valid:
                    break;
            }
        }

        public static List<T> GetAssetList<T>(string path) where T : class
        {
            string[] fileEntries = Directory.GetFiles(path);

            return fileEntries.Select(fileName =>
            {
                string assetPath = fileName.Substring(fileName.IndexOf("Assets"));
                assetPath = Path.ChangeExtension(assetPath, null);
                return UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
            })
                .OfType<T>()
                .ToList();
        }
    }
}
