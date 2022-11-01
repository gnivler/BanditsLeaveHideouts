using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.LinQuick;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace BanditsLeaveHideouts
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly bool MEOWMEOW = Environment.MachineName == "MEOWMEOW";
        private static readonly Harmony Harmony = new("ca.gnivler.bannerlord.ROT.BanditsLeaveHideouts");
        private static readonly Random Rng = new();
        private static CampaignTime MinStay => CampaignTime.Now + CampaignTime.Hours(Rng.Next(0, 72));
        private static readonly Dictionary<MobileParty, CampaignTime> StayTimers = new();
        internal static Settings Settings = new();

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony.PatchAll();
            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ModuleHelper.GetModuleFullPath("BanditsLeaveHideouts") + "settings.json"));
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);
            ((CampaignGameStarter)starterObject).AddBehavior(new BanditLeaveHideoutsBehavior());
            ((CampaignGameStarter)starterObject).AddModel(new ROTBanditDensityModel());
            if (MEOWMEOW)
            {
                CampaignCheats.SetMainPartyAttackable(new List<string> { "0" });
                CampaignCheats.SetCampaignSpeed(new List<string> { "50" });
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            if (MEOWMEOW && Input.IsKeyPressed(InputKey.Tilde))
            {
                var h = Settlement.All.WhereQ(s => s.IsHideout && s.Hideout.IsInfested).GetRandomElementInefficiently();
                MobileParty.MainParty.Position2D = h.Position2D;
                MapScreen.Instance.TeleportCameraToMainParty();
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
            }
        }

        private class BanditLeaveHideoutsBehavior : CampaignBehaviorBase
        {
            public override void RegisterEvents()
            {
                CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
                CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnHourlyTickParty);
            }

            private void OnMobilePartyDestroyed(MobileParty destroyedParty, PartyBase destroyerParty)
            {
                StayTimers.Remove(destroyedParty);
            }

            private void OnHourlyTickParty(MobileParty party)
            {
                if (party.IsBandit && party.CurrentSettlement is not null && party.CurrentSettlement.IsHideout && IsOverInfested(party.CurrentSettlement))
                {
                    if (!StayTimers.TryGetValue(party, out _))
                        StayTimers.Add(party, MinStay);

                    if (CampaignTime.Now.ToHours > StayTimers[party].ToHours)
                    {
                        StayTimers.Remove(party);
                        var hideout = party.CurrentSettlement;
                        party.SetMovePatrolAroundSettlement(hideout);
                    }
                }
            }

            bool IsOverInfested(Settlement hideout) =>
                hideout.Parties.CountQ(x => x.IsBandit) >= Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt + 1;

            public override void SyncData(IDataStore dataStore)
            {
            }
        }
    }
}
