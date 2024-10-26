using Kitchen.Modules;
using KitchenData;
using System.Linq;
using UnityEngine;

namespace KitchenModdedCosmeticsIntegration.Extensions
{
    internal static class GridMenuConfigExtensions
    {
        public static bool AddItemToConfig<T>(this GridMenuConfig config, T item, bool pageHasBack = true)
        {
            Main.LogInfo($"Add {item} to {config.name}");
            if (item is Color color)
            {
                if (config is GridMenuColourConfig colourConfig)
                {
                    colourConfig.Colours.Add(color);
                    return true;
                }
                return false;
            }

            if (item is PlayerCosmetic playerCosmetic)
            {
                if (config is GridMenuCosmeticConfig cosmeticConfig)
                {
                    cosmeticConfig.Cosmetics.Add(playerCosmetic);
                    return true;
                }

                if (config is GridMenuGenericConfig genericConfig)
                {
                    genericConfig.AddItemToGenericConfig(new GridItemCosmetic()
                    {
                        Cosmetic = playerCosmetic
                    }, pageHasBack);
                    return true;
                }

                if (config is GridMenuPaginatedGenericConfig paginatedGenericConfig)
                {
                    paginatedGenericConfig.Items.Add(new GridItemCosmetic()
                    {
                        Cosmetic = playerCosmetic
                    });
                    return true;
                }
                return false;
            }

            if (item is GridMenuConfig gridMenuConfig)
            {
                if (config is GridMenuGenericConfig genericConfig)
                {
                    genericConfig.AddItemToGenericConfig(gridMenuConfig, pageHasBack);
                    return true;
                }

                if (config is GridMenuPaginatedGenericConfig paginatedGenericConfig)
                {
                    paginatedGenericConfig.Items.Add(new GridItemNavigation()
                    {
                        Config = gridMenuConfig
                    });
                    return true;
                }

                if (config is GridMenuNavigationConfig navigationConfig)
                {
                    navigationConfig.AddItemToNavigationConfig(gridMenuConfig, pageHasBack);
                    return true;
                }
                return false;
            }

            if (item is IGridItem gridItem)
            {
                if (config is GridMenuGenericConfig genericConfig)
                {
                    genericConfig.AddItemToGenericConfig(gridItem, pageHasBack);
                    return true;
                }

                if (config is GridMenuPaginatedGenericConfig paginatedGenericConfig)
                {
                    paginatedGenericConfig.Items.Add(gridItem);
                    return true;
                }
            }
            Main.LogError($"Failed to add {item}.");
            return false;
        }

        private static bool AddItemToGenericConfig<T>(this GridMenuGenericConfig genericConfig, T item, bool pageHasBack = true)
        {
            if (genericConfig.Items == null)
                return false;

            if (genericConfig.Items.Count < 7 + (pageHasBack ? 0 : 1))
            {
                if (item is IGridItem gridItem)
                {
                    genericConfig.Items.Add(gridItem);
                    return true;
                }
                else if (item is PlayerCosmetic playerCosmetic)
                {
                    genericConfig.Items.Add(new GridItemCosmetic()
                    {
                        Cosmetic = playerCosmetic
                    });
                    return true;
                }
                else if (item is Dish dish)
                {
                    genericConfig.Items.Add(new GridItemDish()
                    {
                        Dish = dish
                    });
                    return true;
                }
                else if (item is GridMenuConfig gridMenuConfig)
                {
                    genericConfig.Items.Add(new GridItemNavigation()
                    {
                        Config = gridMenuConfig
                    });
                    return true;
                }
                return false;
            }

            if (genericConfig.Items.Last() is GridItemNavigation navigationItem)
            {
                return navigationItem.Config.AddItemToConfig(item);
            }
            return false;
        }


        private static bool AddItemToNavigationConfig(this GridMenuNavigationConfig navigationConfig, GridMenuConfig gridConfig, bool pageHasBack = true)
        {
            if (navigationConfig.Links == null)
                return false;

            if (navigationConfig.Links.Count < 7 + (pageHasBack ? 0 : 1))
            {
                navigationConfig.Links.Add(gridConfig);
                return true;
            }

            return navigationConfig.Links.Last().AddItemToConfig(gridConfig);
        }
    }
}
