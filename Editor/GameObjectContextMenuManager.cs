#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Numeira;

[InitializeOnLoad]
internal static class GameObjectContextMenuManager
{
    static GameObjectContextMenuManager()
    {
        EditorApplication.delayCall += OnDelayCall;
    }

    internal static HashSet<string>? MenuItemRoots;

    static List<MenuItemItem>? OriginalMenuItemItems;

    private const string GameObjectMenuRoot = "GameObject/";
    private const string MenuPathRoot = "Manage Items";

    private static void OnDelayCall()
    {
        // AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(x => x.GetCustomAttribute<MenuItem>() != null);
        var items = new List<MenuItem>();
        items.Clear();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attributes = Attribute.GetCustomAttributes(method, typeof(MenuItem), false);
                    foreach (var attribute in attributes.Cast<MenuItem>())
                    {
                        if (attribute.menuItem.StartsWith(GameObjectMenuRoot))
                            items.Add(attribute);
                    }
                }
            }
        }
        items.Sort((x, y) => x.priority.CompareTo(y.priority));
        MenuItemRoots = items.Select(x =>
        {
            var name = x.menuItem.AsSpan(GameObjectMenuRoot.Length);
            if (name.IndexOf('/') is int index and not -1)
            {
                name = name[..index];
            }
            if (name.IndexOfAny("%$&_") is int index2 and not -1 && name[index2 - 1] == ' ')
            {
                name = name[..(index2 - 1)];
            }
            return name.ToString();
        }).ToHashSet();

        SceneHierarchyHooks.addItemsToGameObjectContextMenu -= OnGameObjectContextMenu;
        SceneHierarchyHooks.addItemsToGameObjectContextMenu += OnGameObjectContextMenu;
    }

    private static void OnGameObjectContextMenu(GenericMenu menu, GameObject @object)
    {
        var configurations = GameObjectContextMenuManagerConfiguration.instance.MenuManageConfigurations;
        if (configurations == null) return;

        var list = Unsafe.As<Tuple<List<MenuItemItem>>>(menu).Item1;

        var miiActualType = list.FirstOrDefault()?.GetType();
        if (miiActualType is null) { return; }

        MenuItemItem CloneMII(MenuItemItem menuItemItem)
        {
            var newInstance = Unsafe.As<MenuItemItem>(Activator.CreateInstance(miiActualType, null, false, true, null));

            newInstance.Content = new(menuItemItem.Content);
            newInstance.Separator = menuItemItem.Separator;
            newInstance.On = menuItemItem.On;
            newInstance.Func = menuItemItem.Func;
            newInstance.Func2 = menuItemItem.Func2;
            newInstance.UserData = menuItemItem.UserData;

            return newInstance;
        }

        OriginalMenuItemItems ??= Unsafe.As<List<MenuItemItem>>(new List<object>(list));
        var editableOrigins = Unsafe.As<List<MenuItemItem>>(new List<object>(OriginalMenuItemItems.Select(CloneMII)));
        list.Clear();

        var listCash = Unsafe.As<List<MenuItemItem>>(new List<object>());

        foreach (var config in configurations)
        {
            if (config.ThisIsSeparator)
            {
                menu.AddSeparator(config.Path);
                continue;
            }
            var basePath = config.Path;
            var includeStatPath = config.IncludeStatPath;

            bool ConfigTarget(MenuItemItem mii)
            {
                var path = mii?.Content?.text;
                if (path is null) { return false; }
                return path.StartsWith(includeStatPath);
            }
            var relocationTarget = listCash;
            relocationTarget.Clear();
            relocationTarget.AddRange(editableOrigins.Where(ConfigTarget));
            foreach (var r in relocationTarget) editableOrigins.Remove(r);

            for (var i = 0; relocationTarget.Count > i; i += 1)
            {
                var menuItemItem = relocationTarget[i];
                var path = menuItemItem?.Content?.text;
                if (path == null) { continue; }
                var relocatedPath = basePath + path.Remove(0, includeStatPath.Length);
                menuItemItem!.Content!.text = relocatedPath;
            }
            list.AddRange(relocationTarget);
        }

        menu.AddSeparator("");

        void AddMenu(string path, GenericMenu.MenuFunction callback, bool enable)
        {
            if (!enable)
            {
                menu.AddDisabledItem(new(path));
                return;
            }
            menu.AddItem(new(path), false, callback);
        }

        AddMenu($"{MenuPathRoot} Edit on GUI..", () => EditorWindow.GetWindow<GUI>(), true);
    }

    internal sealed class MenuItemItem
    {
        public GUIContent? Content;
        public bool Separator;
        public bool On;
        public GenericMenu.MenuFunction? Func;
        public GenericMenu.MenuFunction2? Func2;
        public object? UserData;
    }

    [EditorWindowTitle(title = "ContextMenu Manager")]
    internal sealed class GUI : EditorWindow
    {
        private Vector2 scrollPosition;
        private SerializedObject? sObj;

        public void OnGUI()
        {
            using var ss = new EditorGUILayout.ScrollViewScope(scrollPosition);
            var mmConfiguration = GameObjectContextMenuManagerConfiguration.instance;
            sObj ??= new SerializedObject(mmConfiguration);

            var configurations = sObj.FindProperty(nameof(GameObjectContextMenuManagerConfiguration.MenuManageConfigurations));
            EditorGUILayout.PropertyField(configurations);


            if (GUILayout.Button("Add Separator"))
            {
                configurations.InsertArrayElementAtIndex(0);
                var element = configurations.GetArrayElementAtIndex(0);
                element.FindPropertyRelative(nameof(MenuManageConfiguration.Path)).stringValue = "";
                element.FindPropertyRelative(nameof(MenuManageConfiguration.IncludeStatPath)).stringValue = "";
                element.FindPropertyRelative(nameof(MenuManageConfiguration.ThisIsSeparator)).boolValue = true;
            }


            var menuItemRoots = GameObjectContextMenuManager.MenuItemRoots;
            if (menuItemRoots is not null)
            {
                foreach (var miPath in menuItemRoots)
                {
                    using var s = new GUILayout.HorizontalScope();
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(64f)))
                    {
                        configurations.InsertArrayElementAtIndex(0);
                        var element = configurations.GetArrayElementAtIndex(0);
                        element.FindPropertyRelative(nameof(MenuManageConfiguration.Path)).stringValue = miPath;
                        element.FindPropertyRelative(nameof(MenuManageConfiguration.IncludeStatPath)).stringValue = miPath;
                        element.FindPropertyRelative(nameof(MenuManageConfiguration.ThisIsSeparator)).boolValue = false;
                    }
                    GUILayout.TextField(miPath);
                }
            }
            sObj.ApplyModifiedProperties();

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save to .json"))
                {
                    var writePath = EditorUtility.SaveFilePanel("save GameObjectContextMenuManagerConfiguration", "Assets", "GameObjectContextMenuManagerConfiguration.json", "json");
                    if (string.IsNullOrWhiteSpace(writePath) is false)
                    {
                        File.WriteAllText(writePath, JsonUtility.ToJson(mmConfiguration, true));
                    }
                }
                if (GUILayout.Button("Load from .json"))
                {
                    var writePath = EditorUtility.OpenFilePanel("load GameObjectContextMenuManagerConfiguration", "Assets", "json");
                    if (string.IsNullOrWhiteSpace(writePath) is false)
                    {
                        JsonUtility.FromJsonOverwrite(File.ReadAllText(writePath), mmConfiguration);
                    }
                }
            }
            scrollPosition = ss.scrollPosition;
        }
    }
}
