using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nm1fiOutward.Drops;
using SideLoader;
using SideLoader.SLPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nm1fiOutward.Vegetarian
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SLPlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DropsPlugin.GUID, "0.1.0")] // TODO: set to ^1.0.0 after release and BepInEx support
    public class Vegetarian : BaseUnityPlugin
    {
        public const string GUID = "github.raphendyr.vegetarian";
        public const string NAME = "Vegetarian";
        public const string FEATURE_VERSION = "0.2";
        public const string VERSION = FEATURE_VERSION + ".0";

        // Resource object name is: <project default namespace>.SideLoader.zip
        private const string SIDELOADER_RESOURCE = "Nm1fiOutward.Vegetarian.SideLoader.zip";

        internal void Awake()
        {
            Logger.LogMessage($"Version {VERSION} loading...");

            var asm = Assembly.GetExecutingAssembly();
            var resourceNames = asm.GetManifestResourceNames();
            Logger.LogInfo("List of all bundled resources:\n" + string.Join("\n  ", resourceNames));

#if !DEBUG
            if (resourceNames.Contains(SIDELOADER_RESOURCE))
            {
                // Load SideLoader archive from DLL resources
                var resource = asm.GetManifestResourceStream(SIDELOADER_RESOURCE);
                SLPackArchive.CreatePackFromStream(resource, NAME);
            }
            else
            {
                Logger.LogWarning("Expected resource was not found from the DLL. Is this a development build?");
            }
#endif

            try
            {
                new Harmony(GUID).PatchAll();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Warning, "Exception applying Harmony patches!");
                SL.LogInnerException(e);
            }
        }
    }

    namespace HarmonyPatches
    {
        /// <summary>
        /// Apply effects for crafting items (also with invalid recipes), when crafting is completed.
        /// Effects are applied without animation and only for items in <see cref="ingredientsWithImpliedEffect"/>.
        /// </summary>
        [HarmonyPatch(typeof(CraftingMenu), "CraftingDone")]
        public class CraftingMenu_CraftingDone
        {
            static private readonly HashSet<int> ingredientsWithImpliedEffect = new HashSet<int> {
                4100590, // Gaberry Wine
            };

            public static void Prefix(
                CraftingMenu __instance,
                Recipe.CraftingType ___m_craftingStationType,
                IngredientSelector[] ___m_ingredientSelectors)
            {
                if (___m_craftingStationType != Recipe.CraftingType.Survival)
                {
                    var syncEffectsFor = new Dictionary<int, Item>();
                    foreach (var selector in ___m_ingredientSelectors)
                        if (
                            selector.AssignedIngredient is CompatibleIngredient ingredient
                            && ingredient.ItemID is var ingredientID
                            && ingredientsWithImpliedEffect.Contains(ingredientID)
                            && !syncEffectsFor.ContainsKey(ingredientID)
                            && At.GetField(ingredient, "m_ownedItems") is IList<Item> ingredientItems
                            && ingredientItems.First() is Item item
                        )
                            syncEffectsFor.Add(ingredientID, item);
                    if (syncEffectsFor.Any())
                    {
                        var character = __instance.LocalCharacter;
                        foreach (var item in syncEffectsFor.Values)
                            item.SynchronizeEffects(EffectSynchronizer.EffectCategories.Normal, character);
                    }
                }
            }
        }
    }
}
