#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Numeira
{
    [FilePath("ProjectSettings/GameObjectContextMenuManager.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class GameObjectContextMenuManagerConfiguration : ScriptableSingleton<GameObjectContextMenuManagerConfiguration>
    {
        [SerializeField][Obsolete] private string[]? disabledItems;
        public MenuManageConfiguration[] MenuManageConfigurations = new MenuManageConfiguration[] { MenuManageConfiguration.From("Others", "") };
        public void Save()
        {
            Save(true);
        }
    }

    [Serializable]
    internal struct MenuManageConfiguration
    {
        public string Path;
        public string IncludeStatPath;
        public bool ThisIsSeparator;

        public static MenuManageConfiguration From(string path = "", string includeMenuItemRoot = "") => new MenuManageConfiguration() { Path = path, IncludeStatPath = includeMenuItemRoot };
        public static MenuManageConfiguration Separator(string path = "") => new MenuManageConfiguration() { Path = path, ThisIsSeparator = true };
    }
}
