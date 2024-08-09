using HarmonyLib;
using Kitchen.Modules;
using KitchenData;
using KitchenMods;
using ModdedCosmeticsIntegration.Grids;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace ModdedCosmeticsIntegration
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "ModdedCosmeticsIntegration";
        public const string MOD_VERSION = "0.1.2";

        Harmony _harmony;

        public Main()
        {
            _harmony = new Harmony(MOD_GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        void AddModdedHats()
        {
            int maxHatsPageNumber = -1;
            GridMenuGenericConfig maxHatsPage = null;
            List<PlayerCosmetic> usedCosmetics = new List<PlayerCosmetic>();
            Texture2D hatsIcon = null;
            Texture2D nextPageIcon = Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.name == "menu_right").FirstOrDefault();

            foreach (GridMenuGenericConfig config in Resources.FindObjectsOfTypeAll<GridMenuGenericConfig>())
            {
                if (!config.name.StartsWith("Hats - Page ") ||
                    !int.TryParse(config.name.Last().ToString(), out int hatsPageNumber))
                    continue;

                if (config.name == "Hats - Page 1")
                {
                    hatsIcon = config.Icon;

                    if (nextPageIcon == null)
                    {
                        for (int i = config.Items.Count - 1; i > -1; i--)
                        {
                            if (!(config.Items[i] is GridItemNavigation navItem))
                                continue;
                            nextPageIcon = navItem.Config.Icon;
                            break;
                        }
                    }
                }
                HashSet<GridMenuGenericConfig> seenConfigs = new HashSet<GridMenuGenericConfig>();

                void RecursiveAddUsedCosmetics(GridMenuGenericConfig genericConfig)
                {
                    if (seenConfigs.Contains(genericConfig))
                        return;

                    seenConfigs.Add(genericConfig);
                    foreach (IGridItem gridItem in genericConfig.Items)
                    {
                        if (gridItem is GridItemCosmetic gridItemCosmetic)
                        {
                            usedCosmetics.Add(gridItemCosmetic.Cosmetic);
                            continue;
                        }

                        if (gridItem is GridItemNavigation gridItemNav && gridItemNav.Config)
                        {
                            if (gridItemNav.Config is GridMenuCosmeticConfig cosmeticGridConfig)
                                usedCosmetics.AddRange(cosmeticGridConfig.Cosmetics);
                            else if (gridItemNav.Config is GridMenuGenericConfig childGenericConfig)
                                RecursiveAddUsedCosmetics(childGenericConfig);
                        }
                    }
                }

                RecursiveAddUsedCosmetics(config);

                if (hatsPageNumber > maxHatsPageNumber)
                {
                    maxHatsPage = config;
                    maxHatsPageNumber = hatsPageNumber;
                }
            }

            if (maxHatsPage != null)
            {
                GridMenuGenericConfig gridConfig = null;

                List<PlayerCosmetic> moddedCosmetics = GameData.Main.Get<PlayerCosmetic>().Where(x => (x.CosmeticType == CosmeticType.Hat) && !x.DisableInGame && !usedCosmetics.Contains(x)).ToList();

                for (int i = 0; i < moddedCosmetics.Count; i++)
                {
                    if (gridConfig == null || i % 6 == 0)
                    {
                        GridMenuGenericConfig nextGridConfig = ScriptableObject.CreateInstance<GridMenuGenericConfig>();
                        nextGridConfig.Icon = nextPageIcon;
                        nextGridConfig.name = $"Hats - Modded Page {i / 6 + 1}";
                        nextGridConfig.Items = new List<IGridItem>();
                        if (gridConfig != null)
                        {
                            gridConfig.Items.Add(new GridItemNavigation()
                            {
                                Config = nextGridConfig
                            });
                        }
                        else
                        {
                            nextGridConfig.Icon = hatsIcon;
                            maxHatsPage.Items.Add(new GridItemNavigation()
                            {
                                Config = nextGridConfig
                            });
                        }
                        gridConfig = nextGridConfig;
                    }
                    gridConfig.Items.Add(new GridItemCosmetic()
                    {
                        Cosmetic = moddedCosmetics[i]
                    });
                }
            }
        }

        void AddModdedOutfits()
        {
            foreach (GridMenuNavigationConfig config in Resources.FindObjectsOfTypeAll<GridMenuNavigationConfig>())
            {
                if (config.name == "Root")
                {
                    for (int i = 0; i < config.Links.Count; i++)
                    {
                        if (config.Links[i].name == "Outfits")
                        {
                            CustomGridMenuCosmeticConfig gridConfig = ScriptableObject.CreateInstance<CustomGridMenuCosmeticConfig>();
                            gridConfig.name = "CustomOutfits";
                            gridConfig.Icon = config.Links[i].Icon;
                            gridConfig.Cosmetics = GameData.Main.Get<PlayerCosmetic>().Where(x => x.CosmeticType == CosmeticType.Outfit && !x.DisableInGame).ToList();
                            config.Links[i] = gridConfig;
                        }
                    }
                }
            }
        }

        public void PreInject()
        {
            AddModdedHats();
            AddModdedOutfits();
        }

        //protected void SavePNG(string name, string folderName, byte[] bytes)
        //{
        //    string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }
        //    File.WriteAllBytes(Path.Combine(folderPath, name), bytes);
        //}
        public void PostInject()
        {
            //    foreach (PlayerCosmetic cosmetic in GameData.Main.Get<PlayerCosmetic>())
            //    {
            //        Texture2D snapshot = PrefabSnapshot.GetCosmeticSnapshot(cosmetic);
            //        byte[] bytes = snapshot.EncodeToPNG();
            //        string filename = $"{cosmetic.ID}";
            //        SavePNG(filename, "Cosmetics", bytes);
            //    }
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
