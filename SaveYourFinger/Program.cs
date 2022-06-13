using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins;

namespace SaveYourFinger
{
    public class Program
    {
        private readonly ILoadOrder<IModListing<IFallout4ModGetter>> LoadOrder;

        private readonly IFallout4Mod PatchMod;

        private static readonly Weapon.Flag invalidWeaponFlags =
            Weapon.Flag.NotPlayable |
            Weapon.Flag.NotUsedInNormalCombat |
            Weapon.Flag.EmbeddedWeapon |
            Weapon.Flag.Automatic |
            Weapon.Flag.RepeatableSingleFire
            ;

        private readonly IFormLinkGetter<IEquipTypeGetter> GrenadeSlot = ModKey.FromNameAndExtension("Fallout4.esm").MakeFormKey(0x046AAC).AsLinkGetter<IEquipTypeGetter>();

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "YourPatcher.esp")
                .Run(args);
        }

        private static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state) => new Program(state.LoadOrder, state.PatchMod).Run();

        private Program(
            ILoadOrder<IModListing<IFallout4ModGetter>> loadOrder,
            IFallout4Mod patchMod)
        {
            LoadOrder = loadOrder;
            PatchMod = patchMod;
        }

        private void Run()
        {
            foreach (var weap in LoadOrder.PriorityOrder.Weapon().WinningOverrides())
            {
                if (weap.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)) continue;
                if ((weap.Flags & invalidWeaponFlags) != 0) continue;
                // TODO Fallout4.EquipType.GrenadeSlot
                if (weap.EquipmentType.Equals(GrenadeSlot)) continue;

                var modifiedWeapon = PatchMod.Weapons.GetOrAddAsOverride(weap);

                if (!modifiedWeapon.Ammo.IsNull)
                    modifiedWeapon.Flags |= Weapon.Flag.RepeatableSingleFire;
                else
                    modifiedWeapon.Flags |= Weapon.Flag.Automatic;
            }
        }
    }
}
