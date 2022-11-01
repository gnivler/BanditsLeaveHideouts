using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace BanditsLeaveHideouts;

public class ROTBanditDensityModel : DefaultBanditDensityModel
{
    public override int NumberOfMaximumLooterParties => SubModule.Settings.NumberOfMaximumLooterParties;
    public override int NumberOfMaximumHideoutsAtEachBanditFaction => SubModule.Settings.NumberOfMaximumHideoutsAtEachBanditFaction;
    public override int NumberOfInitialHideoutsAtEachBanditFaction => SubModule.Settings.NumberOfInitialHideoutsAtEachBanditFaction;
    public override int NumberOfMaximumBanditPartiesAroundEachHideout => SubModule.Settings.NumberOfMaximumBanditPartiesAroundEachHideout;
}
