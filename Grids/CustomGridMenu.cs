using Controllers;
using HarmonyLib;
using Kitchen;
using Kitchen.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace ModdedCosmeticsIntegration.Grids
{
    [HarmonyPatch]
    static class CustomGridMenu_Patch
    {
        [HarmonyPatch(typeof(GridMenu), nameof(GridMenu.HandleInteraction))]
        [HarmonyPrefix]
        static bool HandleInteraction_Prefix(ref GridMenu __instance, InputState state, ref bool __result)
        {
            if (!(__instance is CustomGridMenu customGridMenu))
                return true;
            
            int selectedIndex = customGridMenu.Grid.Modules.IndexOf(customGridMenu.Grid.Selected);
            int currentPage = customGridMenu.Page;

            if (state.MenuDown == ButtonState.Pressed &&
                selectedIndex + customGridMenu.RowLength > customGridMenu.MaxPerGroup - 1 &&
                customGridMenu.Page < customGridMenu.PageCount - 1)
            {
                customGridMenu.Redraw(currentPage + 1);
                __result = true;
                return false;
            }
            if (state.MenuUp == ButtonState.Pressed &&
                selectedIndex - customGridMenu.RowLength < 0 &&
                customGridMenu.Page > 0)
            {
                customGridMenu.Redraw(currentPage - 1);
                __result = true;
                return false;
            }
            if (state.MenuRight == ButtonState.Pressed &&
                selectedIndex + 1 > customGridMenu.MaxPerGroup - 1 &&
                customGridMenu.Page < customGridMenu.PageCount - 1)
            {
                customGridMenu.Redraw(currentPage + 1, customGridMenu.MaxPerGroup - customGridMenu.RowLength);
                __result = true;
                return false;
            }
            if (state.MenuLeft == ButtonState.Pressed &&
                selectedIndex - 1 < 0 &&
                customGridMenu.Page > 0)
            {
                customGridMenu.Redraw(currentPage - 1, customGridMenu.RowLength - 1);
                __result = true;
                return false;
            }
            if (state.GrabAction == ButtonState.Pressed)
                Main.LogInfo("Grab Pressed");
            return true;

        }
    }

    public abstract class CustomGridMenu : GridMenu
    {
        public virtual int RowLength => 4;
        public virtual int ColumnLength => 2;
        public int MaxPerGroup => RowLength * ColumnLength;
        public virtual int PageCount => Mathf.CeilToInt(ElementCount / ((float)RowLength)) - 1;
        public virtual int Page { get; protected set; } = 0;
        public virtual int ElementCount => ItemCount + (HasBack ? 1 : 0);
        public virtual int ItemCount => 0;
        public virtual bool HasBack { get; protected set; } = false;
        public abstract void Redraw(int page, int selectedIndex = -1);
    }

    public abstract class CustomGridMenu<TItem> : CustomGridMenu
    {
        protected virtual float ElementWidth => 0.5f;
        protected virtual float ElementHeight => 0.5f;
        protected virtual float Padding => 0.1f;
        public override int ItemCount => Items?.Count ?? 0;
        protected List<TItem> Items;

        protected virtual GridMenuElement GetPrefab()
        {
            return ModuleDirectory.Main.GetPrefab<GridMenuElement>();
        }

        public CustomGridMenu(List<TItem> items, Transform container, int player, bool has_back)
        {
            Container = container;
            Player = player;
            Panel = UnityEngine.Object.Instantiate(ModuleDirectory.Main.GetPrefab<PanelElement>(), container, worldPositionStays: false);
            Page = 0;
            Items = items;
            HasBack = has_back;
            CreateElements();
        }

        public override void Redraw(int page, int selectedIndex = -1)
        {
            Page = page;
            if (selectedIndex == -1)
                selectedIndex = Grid.Modules.IndexOf(Grid.Selected);
            Grid.Destroy();
            CreateElements(page);
            for (int i = selectedIndex; i > -1; i--)
            {
                if (Grid.Modules.Count > i && Grid.Modules[i].Module.IsSelectable)
                {
                    Grid.Select(Grid.Modules[i].Module);
                    break;
                }
            }
        }

        protected void CreateElements(int page = 0)
        {
            GridMenuElement prefab = GetPrefab();
            Grid = new ModuleGrid
            {
                RowLength = RowLength,
                ColumnLength = ColumnLength,
                XSpacing = ElementWidth,
                YSpacing = ElementHeight,
                Padding = Padding
            };
            int drawnCount = 0;
            int itemIndex = (page * RowLength) - (HasBack ? 1 : 0);
            for (; itemIndex < ItemCount; itemIndex++)
            {
                GridMenuElement gridMenuElement = UnityEngine.Object.Instantiate(prefab, Container, worldPositionStays: false);
                if (itemIndex == -1)
                {
                    if (page == 0 && HasBack)
                    {
                        gridMenuElement.OnActivate += base.RequestGoBack;
                        gridMenuElement.SetAsBack();
                    }
                    else
                        continue;
                }
                else
                {
                    TItem item = Items[itemIndex];
                    SetupElement(item, gridMenuElement);
                    gridMenuElement.OnActivate += delegate
                    {
                        OnSelect(item);
                    };
                }
                Grid.AddModule(gridMenuElement);

                if (++drawnCount >= MaxPerGroup)
                {
                    break;
                }
            }
            for (; drawnCount < MaxPerGroup; drawnCount++)
            {
                GridMenuElement gridMenuElement3 = UnityEngine.Object.Instantiate(prefab, Container, worldPositionStays: false);
                gridMenuElement3.SetSelectable(selectable: false);
                Grid.AddModule(gridMenuElement3);
            }
            Panel.SetTarget(Grid);
            Panel.SetColour(Player);
        }

        protected abstract void SetupElement(TItem item, GridMenuElement element);

        protected abstract void OnSelect(TItem item);
    }
}
