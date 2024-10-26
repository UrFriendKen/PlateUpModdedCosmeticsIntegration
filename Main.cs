using HarmonyLib;
using Kitchen.Modules;
using KitchenData;
using KitchenModdedCosmeticsIntegration.Extensions;
using KitchenMods;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenModdedCosmeticsIntegration
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "ModdedCosmeticsIntegration";
        public const string MOD_VERSION = "0.1.4";

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
        
        HashSet<int> SeenGridItemGDOIDs = new HashSet<int>();

        void InitialiseSeenGridItemGDOs()
        {
            SeenGridItemGDOIDs.Clear();
            foreach (GridMenuConfig config in Resources.FindObjectsOfTypeAll<GridMenuConfig>())
            {
                Main.LogInfo(config.name);
                if (config is GridMenuGenericConfig genericConfig)
                {
                    foreach (IGridItem gridItem in genericConfig.Items)
                    {
                        int gdoID = 0;
                        if (gridItem is GridItemCosmetic cosmeticGridItem)
                        {
                            if (cosmeticGridItem.Cosmetic != null)
                                Main.LogInfo($"\t{cosmeticGridItem.Cosmetic.name}");
                            gdoID = cosmeticGridItem.Cosmetic?.ID ?? 0;
                        }
                        else if (gridItem is GridItemDish dishGridItem)
                            gdoID = dishGridItem.Dish?.ID ?? 0;

                        if (gdoID != 0)
                            SeenGridItemGDOIDs.Add(gdoID);
                    }
                }
                else if (config is GridMenuCosmeticConfig cosmeticConfig)
                {
                    foreach (PlayerCosmetic cosmetic in cosmeticConfig.Cosmetics)
                    {
                        if (cosmetic?.ID == 0)
                            continue;
                        Main.LogInfo($"\t{cosmetic.name}");
                        SeenGridItemGDOIDs.Add(cosmetic.ID);
                    }
                }
            }
        }

        public void DoingThemAFavorToHideFutureContentCusPlateUpsBuiltInSolutionIsFreakingPooPoo()
        {
            foreach (int cosmeticID in new int[]
                {
                    //133088131,
                    1190448658,
                    1623972248,
                    -1382700210,
                    655614579,
                    -439502309,
                    -545572929,
                    1538718644,
                    907728224,
                    153589827,
                    -1773267544,
                    -30734782,
                    -1627755214,
                    732564270,
                    1453139625,
                    913958454,
                    -1776995498,
                    -1372763235,
                    -1015061923,
                    991572677,
                    751635467,
                    1937347555,
                    429857619
                })
            {
                SeenGridItemGDOIDs.Add(cosmeticID);
            }
        }

        void AddModdedOutfitsPaginated()
        {
            foreach (GridMenuConfig config in Resources.FindObjectsOfTypeAll<GridMenuConfig>())
            {
                if (config.name != "Root")
                    continue;

                if (!(config is GridMenuNavigationConfig navigationConfig))
                    continue;


                for (int i = 0; i < navigationConfig.Links.Count; i++)
                {
                    if (navigationConfig.Links[i].name.StartsWith("Outfits"))
                    {
                        GridMenuPaginatedGenericConfig moddedOutfitsConfig = ScriptableObject.CreateInstance<GridMenuPaginatedGenericConfig>();
                        moddedOutfitsConfig.name = "ModdedCosmeticsIntegration_Outfits";

                        Texture2D baseTexture = navigationConfig.Links[i].Icon;
                        moddedOutfitsConfig.Icon = baseTexture;

                        List<IGridItem> items = new List<IGridItem>();
                        Main.LogInfo("Populating modded outfits");
                        int count = 0;
                        foreach (PlayerCosmetic cosmetic in GameData.Main.Get<PlayerCosmetic>())
                        {
                            if (SeenGridItemGDOIDs.Contains(cosmetic.ID) ||
                                cosmetic.CosmeticType != CosmeticType.Outfit ||
                                cosmetic.DisableInGame)
                                continue;
                            Main.LogInfo($"\t{cosmetic.name} ({cosmetic.ID})");
                            items.Add(new GridItemCosmetic()
                            {
                                Cosmetic = cosmetic
                            });
                            count++;
                        }

                        if (items.Count == 0)
                        {
                            Main.LogInfo("No modded outfits to add. Skipping.");
                            break;
                        }

                        Main.LogInfo($"\tAdded {count} outifts!");
                        moddedOutfitsConfig.Items = items;

                        if (!navigationConfig.Links[i].AddItemToConfig(moddedOutfitsConfig))
                            navigationConfig.AddItemToConfig(moddedOutfitsConfig);
                        break;
                    }
                }
            }
        }

        void AddModdedHatsPaginated()
        {
            foreach (GridMenuConfig config in Resources.FindObjectsOfTypeAll<GridMenuConfig>())
            {
                if (config.name != "Root")
                    continue;

                if (!(config is GridMenuNavigationConfig navigationConfig))
                    continue;


                for (int i = 0; i < navigationConfig.Links.Count; i++)
                {
                    if (navigationConfig.Links[i].name.StartsWith("Hats"))
                    {
                        GridMenuPaginatedGenericConfig moddedHatsConfig = ScriptableObject.CreateInstance<GridMenuPaginatedGenericConfig>();
                        moddedHatsConfig.name = "ModdedCosmeticsIntegration_Hats";
                        moddedHatsConfig.Icon = navigationConfig.Links[i].Icon;
                        List<IGridItem> items = new List<IGridItem>();
                        Main.LogInfo("Populating modded hats");
                        int count = 0;
                        foreach (PlayerCosmetic cosmetic in GameData.Main.Get<PlayerCosmetic>())
                        {
                            if (SeenGridItemGDOIDs.Contains(cosmetic.ID) ||
                                cosmetic.CosmeticType != CosmeticType.Hat ||
                                cosmetic.DisableInGame)
                                continue;
                            Main.LogInfo($"\t{cosmetic.name} ({cosmetic.ID})");
                            items.Add(new GridItemCosmetic()
                            {
                                Cosmetic = cosmetic
                            });
                            count++;
                        }

                        if (items.Count == 0)
                        {
                            Main.LogInfo("No modded hats to add. Skipping.");
                            break;
                        }

                        Main.LogInfo($"\tAdded {count} hats!");
                        moddedHatsConfig.Items = items;

                        if (!navigationConfig.Links[i].AddItemToConfig(moddedHatsConfig))
                            navigationConfig.AddItemToConfig(moddedHatsConfig);
                        break;
                    }
                }
            }
        }

        public void PreInject()
        {
            InitialiseSeenGridItemGDOs();

            DoingThemAFavorToHideFutureContentCusPlateUpsBuiltInSolutionIsFreakingPooPoo();

            AddModdedHatsPaginated();
            AddModdedOutfitsPaginated();
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
