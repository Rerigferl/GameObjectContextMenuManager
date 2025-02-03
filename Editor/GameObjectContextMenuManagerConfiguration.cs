using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Numeira
{
    [FilePath("ProjectSettings/GameObjectContextMenuManager.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class GameObjectContextMenuManagerConfiguration : ScriptableSingleton<GameObjectContextMenuManagerConfiguration>
    {
        [SerializeField]
        private string[]? disabledItems;

        private HashSet<string>? disabledItemsBackfield;
        public HashSet<string> DisabledItems
        {
            get
            {
                return disabledItemsBackfield ??= (disabledItems ??= new string[0]).ToHashSet();
            }
        }

        public void Save()
        {
            disabledItems = disabledItemsBackfield.ToArray();
            Save(true);
        }
    }
}