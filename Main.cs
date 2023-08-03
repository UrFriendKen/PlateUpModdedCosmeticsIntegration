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
        public const string MOD_VERSION = "0.1.0";

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

        public void PreInject()
        {
            foreach (GridMenuGenericConfig config in Resources.FindObjectsOfTypeAll<GridMenuGenericConfig>())
            {
                if (config.name == "Hats")
                {
                    bool found = false;
                    List<PlayerCosmetic> used = new List<PlayerCosmetic>();
                    for (int i = 0; i < config.Items.Count; i++)
                    {
                        if (config.Items[i] is GridItemCosmetic gridItemCosmetic)
                        {
                            used.Add(gridItemCosmetic.Cosmetic);
                            continue;
                        }
                        if (config.Items[i] is GridItemNavigation gridItemNav && gridItemNav.Config is GridMenuCosmeticConfig cosmeticGridConfig)
                        {
                            used.AddRange(cosmeticGridConfig.Cosmetics);
                            continue;
                        }
                        if (config.Items[i] is CustomGridMenuCosmeticConfig customCosmeticGrid)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        CustomGridMenuCosmeticConfig gridConfig = ScriptableObject.CreateInstance<CustomGridMenuCosmeticConfig>();
                        gridConfig.name = "Hats - Modded";
                        gridConfig.Icon = config.Icon;
                        gridConfig.Cosmetics = GameData.Main.Get<PlayerCosmetic>().Where(x => (x.CosmeticType == CosmeticType.Hat) && !x.DisableInGame && !used.Contains(x)).ToList();
                        config.Items.Add(new GridItemNavigation()
                        {
                            Config = gridConfig
                        });
                    }
                }
            }

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

                        if (config.Links[i].name == "Hats" && config.Links[i] is GridMenuGenericConfig hatsConfig)
                        {
                            CustomGridMenuGenericConfig gridConfig = ScriptableObject.CreateInstance<CustomGridMenuGenericConfig>();
                            gridConfig.name = "CustomHats";
                            gridConfig.Icon = config.Links[i].Icon;
                            gridConfig.Items = hatsConfig.Items;
                            config.Links[i] = gridConfig;
                        }
                    }
                }
            }
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
