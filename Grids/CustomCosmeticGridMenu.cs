using Kitchen;
using Kitchen.Modules;
using KitchenData;
using System.Collections.Generic;
using UnityEngine;

namespace ModdedCosmeticsIntegration.Grids
{
    public class CustomCosmeticGridMenu : CustomGridMenu<PlayerCosmetic>
    {
        public CustomCosmeticGridMenu(List<PlayerCosmetic> cosmetics, Transform container, int player, bool has_back)
            : base(cosmetics, container, player, has_back)
        {
        }

        protected override void SetupElement(PlayerCosmetic item, GridMenuElement element)
        {
            element.Set(item);
        }

        protected override void OnSelect(PlayerCosmetic cosmetic)
        {
            if (Player != 0 && cosmetic != null)
            {
                ProfileAccessor.SetCosmetic(Player, cosmetic);
            }
        }
    }

    public class CustomGridMenuCosmeticConfig : GridMenuConfig
    {
        public List<PlayerCosmetic> Cosmetics;

        public override GridMenu Instantiate(Transform container, int player, bool has_back)
        {
            return new CustomCosmeticGridMenu(Cosmetics, container, player, has_back);
        }
    }
}
