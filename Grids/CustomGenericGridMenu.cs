using Kitchen;
using Kitchen.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace ModdedCosmeticsIntegration.Grids
{
    public class CustomGenericGridMenu : CustomGridMenu<IGridItem>
    {
        public CustomGenericGridMenu(List<IGridItem> cosmetics, Transform container, int player, bool has_back)
            : base(cosmetics, container, player, has_back)
        {
        }

        protected override void SetupElement(IGridItem item, GridMenuElement element)
        {
            element.Set(item);
        }

        protected override void OnSelect(IGridItem item)
        {
            if (!(item is GridItemNavigation gridItemNavigation))
            {
                if (item is GridItemCosmetic)
                {
                    GridItemCosmetic gridItemCosmetic = (GridItemCosmetic)(object)item;
                    if (Player != 0 && gridItemCosmetic.Cosmetic != null)
                    {
                        ProfileAccessor.SetCosmetic(Player, gridItemCosmetic.Cosmetic);
                    }
                }
            }
            else if (gridItemNavigation.Config != null)
            {
                RequestNewMenu(gridItemNavigation.Config);
            }
        }
    }

    public class CustomGridMenuGenericConfig : GridMenuConfig
    {
        public List<IGridItem> Items;

        public override GridMenu Instantiate(Transform container, int player, bool has_back)
        {
            return new CustomGenericGridMenu(Items, container, player, has_back);
        }
    }
}
