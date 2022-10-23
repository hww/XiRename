/* Copyright (c) 2018 Valeriya Pudova (hww.github.io) Reading lisense file */

using System.Collections.Generic;
using UnityEngine;

namespace XiRenameTool.Utils
{
    /// <summary>A unique name tools helps to build unique name for objects.</summary>
    public static class UniqueRenamableTool
    {
        /// <summary>(Immutable) identifier for the maximum object.</summary>
        private const int MaxObjectId = 999;

        /// <summary>All list of all comparable objects to find unique name for.</summary>
        static Dictionary<string, RenamableObject> sObjectsTable = new Dictionary<string, RenamableObject>();


        ///--------------------------------------------------------------------
        /// <summary>Makes unique name of object in the group.</summary>
        ///
        /// <param name="objects">       The objects to prevent their names.</param>
        /// <param name="objectToRename">The object to rename.</param>
        ///--------------------------------------------------------------------

        public static string MakeUniqueName(List<RenamableObject> objects, string format = "0", bool addNumberToZero = false)
        {
            sObjectsTable.Clear();
            var separator = XiRename.GetSeparator();
            List<RenamableObject> toRename = new List<RenamableObject>();
            foreach (var obj in objects)
            {

                var fileNumber = new FileNumber(obj.FileName);
                if (addNumberToZero && fileNumber.id == 0)
                {
                    toRename.Add(obj);
                }
                else
                {
                    var newName = fileNumber.GetString(format, addNumberToZero, separator);
                    RenamableObject existingObject = null;
                    if (sObjectsTable.TryGetValue(newName, out existingObject))
                    {
                        toRename.Add(obj);  // add it to rename list
                    }
                    else
                    {
                        obj.ResultOrCustomName = newName;
                        sObjectsTable[newName] = obj;
                    }
                }
            }
            foreach (var obj in toRename)
            {

                var found = false;
                var fileNumber = new FileNumber(obj.FileName);
                for (var nid = 0; nid < MaxObjectId; nid++)
                {
                    var newName = fileNumber.GetString(nid, format, true, separator);
                    RenamableObject existingObject = null;
                    if (sObjectsTable.TryGetValue(newName, out existingObject))
                        continue;
                    found = true;
                    obj.ResultOrCustomName = newName;
                    sObjectsTable[newName] = obj;
                    break;
                }
                if (!found)
                    Debug.LogError($"Can't build unique name for object '{obj.FileName}'");
            }

            sObjectsTable.Clear();
            return null;
        }

        ///--------------------------------------------------------------------
        /// <summary>Makes name with suffix numerical nameNumb.</summary>
        ///
        /// <param name="nameWithoutId">.</param>
        /// <param name="id">           .</param>
        ///
        /// <returns>The name with identifier.</returns>
        ///--------------------------------------------------------------------

        public static string GetNameWithId(string nameWithoutId, int id, char separator, bool addNumberToZero, string format)
        {
            if (addNumberToZero)
                return $"{nameWithoutId}{separator}{id.ToString(format)}";

            // when ID==1 the name can be without ID suffix
            return id == 0 ? nameWithoutId : $"{nameWithoutId}{separator}{id.ToString(format)}";
        }

    }
}