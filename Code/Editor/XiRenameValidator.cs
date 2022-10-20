using System.Collections;
using System.Collections.Generic;
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
    }
}
