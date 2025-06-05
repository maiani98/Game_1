using NUnit.Framework;
using UnityEngine;
using ChaosCosmos.Services.PassSystem;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Gameplay.Progression; // Per UpgradeData usato in RewardData
using ChaosCosmos.Tests.EditMode.Services.PassSystem.Mocks;
using System.Collections.Generic;
using System.Linq;

namespace ChaosCosmos.Tests.EditMode.Services.PassSystem
{
    public class PassManagerTests
    {
        private PassManager _passManager;
        private MockProgressService _mockProgressService;
        private PassSeasonData _testSeasonData;
        private RewardData _xpReward;
        private RewardData _upgradeReward;
        private UpgradeData _testUpgradeDataSO; // Rinominato per chiarezza che è un SO

        // Helper per chiavi PlayerPrefs specifiche per la stagione di test
        private string GetTestXPKey(string seasonID) => $"ChaosCosmos_PassSystem_{seasonID}_XP";
        private string GetTestPremiumKey(string seasonID) => $"ChaosCosmos_PassSystem_{seasonID}_HasPremium";
        private string GetTestClaimedKey(string seasonID, int tier, bool premium, string rewardId) =>
            $"ChaosCosmos_PassSystem_{seasonID}_Tier{tier}_{(premium ? "P" : "F")}_{rewardId}_Claimed";

        [SetUp]
        public void SetUp()
        {
            _mockProgressService = new MockProgressService();
            // PassManager ora prende IProgressService nel costruttore
            _passManager = new PassManager(_mockProgressService);

            // Creare dati di test ScriptableObject
            _xpReward = ScriptableObject.CreateInstance<RewardData>();
            _xpReward.rewardID = "test_xp_100";
            _xpReward.type = RewardType.XPCurrency;
            _xpReward.amount = 100;
            _xpReward.displayName = "100 XP";

            _testUpgradeDataSO = ScriptableObject.CreateInstance<UpgradeData>();
            _testUpgradeDataSO.upgradeID = "test_upgrade_01";
            _testUpgradeDataSO.upgradeName = "Test Upgrade";

            _upgradeReward = ScriptableObject.CreateInstance<RewardData>();
            _upgradeReward.rewardID = "test_upg_reward";
            _upgradeReward.type = RewardType.SpecificUpgrade;
            _upgradeReward.upgradeToGrant = _testUpgradeDataSO; // Assegna l'SO creato
            _upgradeReward.displayName = "Free Test Upgrade";

            var tier1 = ScriptableObject.CreateInstance<PassTierData>();
            tier1.tierLevel = 1;
            tier1.xpToUnlockThisTier = 100;
            tier1.freeRewards = new List<RewardData> { _xpReward };
            tier1.premiumRewards = new List<RewardData>();

            var tier2 = ScriptableObject.CreateInstance<PassTierData>();
            tier2.tierLevel = 2;
            tier2.xpToUnlockThisTier = 150;
            tier2.freeRewards = new List<RewardData>();
            tier2.premiumRewards = new List<RewardData> { _upgradeReward };

            _testSeasonData = ScriptableObject.CreateInstance<PassSeasonData>();
            _testSeasonData.seasonID = "TestSeason_PassManager"; // ID univoco per i test
            _testSeasonData.seasonName = "Test Season for PassManager";
            _testSeasonData.tiers = new List<PassTierData> { tier1, tier2 };

            // Pulisci PlayerPrefs prima di Initialize per assicurare uno stato pulito per il caricamento
            PlayerPrefs.DeleteKey(GetTestXPKey(_testSeasonData.seasonID));
            PlayerPrefs.DeleteKey(GetTestPremiumKey(_testSeasonData.seasonID));
            if(_xpReward != null && !string.IsNullOrEmpty(_xpReward.rewardID))
                PlayerPrefs.DeleteKey(GetTestClaimedKey(_testSeasonData.seasonID, 1, false, _xpReward.rewardID));
            if(_upgradeReward != null && !string.IsNullOrEmpty(_upgradeReward.rewardID))
                PlayerPrefs.DeleteKey(GetTestClaimedKey(_testSeasonData.seasonID, 2, true, _upgradeReward.rewardID));
            PlayerPrefs.Save();


            _passManager.Initialize(_testSeasonData);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(GetTestXPKey(_testSeasonData.seasonID));
            PlayerPrefs.DeleteKey(GetTestPremiumKey(_testSeasonData.seasonID));
            if(_xpReward != null && !string.IsNullOrEmpty(_xpReward.rewardID))
                 PlayerPrefs.DeleteKey(GetTestClaimedKey(_testSeasonData.seasonID, 1, false, _xpReward.rewardID));
            if(_upgradeReward != null && !string.IsNullOrEmpty(_upgradeReward.rewardID))
                 PlayerPrefs.DeleteKey(GetTestClaimedKey(_testSeasonData.seasonID, 2, true, _upgradeReward.rewardID));
            PlayerPrefs.Save();

            Object.DestroyImmediate(_xpReward);
            Object.DestroyImmediate(_upgradeReward);
            Object.DestroyImmediate(_testUpgradeDataSO);
            if (_testSeasonData != null && _testSeasonData.tiers != null)
            {
                foreach(var tier in _testSeasonData.tiers) { if(tier != null) Object.DestroyImmediate(tier); }
            }
            if(_testSeasonData != null) Object.DestroyImmediate(_testSeasonData);
        }

        [Test]
        public void Initialize_CalculatesCumulativeXPAndSetsInitialState()
        {
            Assert.AreEqual(0, _passManager.CurrentPassXP, "Initial Pass XP should be 0.");
            Assert.AreEqual(0, _passManager.CurrentPassTier, "Initial Pass Tier should be 0.");
            Assert.IsFalse(_passManager.HasPremiumPass, "Initial Premium Pass status should be false.");
        }

        [Test]
        public void AddPassXP_UpdatesXPAndTierCorrectly()
        {
            _passManager.AddPassXP(100);
            Assert.AreEqual(100, _passManager.CurrentPassXP);
            Assert.AreEqual(1, _passManager.CurrentPassTier);

            _passManager.AddPassXP(100);
            Assert.AreEqual(200, _passManager.CurrentPassXP);
            Assert.AreEqual(1, _passManager.CurrentPassTier);

            _passManager.AddPassXP(50);
            Assert.AreEqual(250, _passManager.CurrentPassXP);
            Assert.AreEqual(2, _passManager.CurrentPassTier);
        }

        [Test]
        public void AddPassXP_DoesNotAddNegativeOrZeroXP()
        {
            _passManager.AddPassXP(0);
            Assert.AreEqual(0, _passManager.CurrentPassXP);
            _passManager.AddPassXP(-50);
            Assert.AreEqual(0, _passManager.CurrentPassXP);
        }

        [Test]
        public void IsTierUnlocked_ReturnsCorrectStatus()
        {
            Assert.IsFalse(_passManager.IsTierUnlocked(1));
            _passManager.AddPassXP(100);
            Assert.IsTrue(_passManager.IsTierUnlocked(1));
            Assert.IsFalse(_passManager.IsTierUnlocked(2));
            _passManager.AddPassXP(150);
            Assert.IsTrue(_passManager.IsTierUnlocked(2));
        }

        [Test]
        public void ClaimReward_FreeReward_Success()
        {
            _passManager.AddPassXP(100);
            bool claimed = _passManager.ClaimReward(1, false, _xpReward);
            Assert.IsTrue(claimed, "Claim should be successful.");
            Assert.AreEqual(100, _mockProgressService.XPGained, "XP should be granted via ProgressService.");
            Assert.IsTrue(_passManager.IsRewardClaimed(1, false, _xpReward), "Reward should be marked as claimed.");
        }

        [Test]
        public void ClaimReward_PremiumReward_Fail_WhenNoPremiumPass()
        {
            _passManager.AddPassXP(250);
            bool claimed = _passManager.ClaimReward(2, true, _upgradeReward);
            Assert.IsFalse(claimed, "Claim should fail without premium pass.");
            Assert.IsNull(_mockProgressService.LastGrantedUpgrade, "Upgrade should not be granted.");
            Assert.IsFalse(_passManager.IsRewardClaimed(2, true, _upgradeReward), "Reward should not be marked as claimed.");
        }

        [Test]
        public void ClaimReward_PremiumReward_Success_WithPremiumPass()
        {
            _passManager.PurchasePremiumPass();
            _passManager.AddPassXP(250);
            bool claimed = _passManager.ClaimReward(2, true, _upgradeReward);
            Assert.IsTrue(claimed, "Claim should be successful with premium pass.");
            Assert.AreEqual(_testUpgradeDataSO, _mockProgressService.LastGrantedUpgrade, "Upgrade should be granted.");
            Assert.IsTrue(_passManager.IsRewardClaimed(2, true, _upgradeReward), "Reward should be marked as claimed.");
        }

        [Test]
        public void ClaimReward_Fail_IfTierNotUnlocked()
        {
            bool claimed = _passManager.ClaimReward(1, false, _xpReward);
            Assert.IsFalse(claimed, "Claim should fail if tier is not unlocked.");
        }

        [Test]
        public void ClaimReward_Fail_IfAlreadyClaimed()
        {
            _passManager.AddPassXP(100);
            _passManager.ClaimReward(1, false, _xpReward);
            _mockProgressService.ResetTestState();

            bool claimedAgain = _passManager.ClaimReward(1, false, _xpReward);
            Assert.IsFalse(claimedAgain, "Claiming an already claimed reward should fail.");
            Assert.AreEqual(0, _mockProgressService.XPGained, "No XP should be granted on second attempt.");
        }

        [Test]
        public void PurchasePremiumPass_SetsFlagAndSaves()
        {
            _passManager.PurchasePremiumPass();
            Assert.IsTrue(_passManager.HasPremiumPass, "HasPremiumPass flag should be true.");
            Assert.AreEqual(1, PlayerPrefs.GetInt(GetTestPremiumKey(_testSeasonData.seasonID)), "PlayerPrefs premium key should be set to 1.");
        }

        [Test]
        public void SaveAndLoadPassProgress_RestoresState()
        {
            _passManager.AddPassXP(120);
            _passManager.PurchasePremiumPass();
            _passManager.ClaimReward(1, false, _xpReward);

            var newPassManager = new PassManager(_mockProgressService);
            newPassManager.Initialize(_testSeasonData);

            Assert.AreEqual(120, newPassManager.CurrentPassXP, "XP not restored correctly.");
            Assert.AreEqual(1, newPassManager.CurrentPassTier, "Tier not restored correctly.");
            Assert.IsTrue(newPassManager.HasPremiumPass, "Premium status not restored.");
            Assert.IsTrue(newPassManager.IsRewardClaimed(1, false, _xpReward), "Claimed status of free reward not restored.");
            Assert.IsFalse(newPassManager.IsRewardClaimed(2, true, _upgradeReward), "Claimed status of unclaimed premium reward incorrect.");
        }

        [Test]
        public void ResetPassProgress_ClearsData()
        {
            _passManager.AddPassXP(100);
            _passManager.PurchasePremiumPass();
            _passManager.ClaimReward(1, false, _xpReward);

            _passManager.ResetPassProgress();

            Assert.AreEqual(0, _passManager.CurrentPassXP, "XP should be 0 after reset.");
            Assert.AreEqual(0, _passManager.CurrentPassTier, "Tier should be 0 after reset.");
            Assert.IsFalse(_passManager.HasPremiumPass, "Premium status should be false after reset.");
            Assert.IsFalse(_passManager.IsRewardClaimed(1, false, _xpReward), "Reward claim status should be false after reset.");
            Assert.AreEqual(0, PlayerPrefs.GetInt(GetTestXPKey(_testSeasonData.seasonID), -1), "XP PlayerPref should be cleared or 0.");
            Assert.AreEqual(0, PlayerPrefs.GetInt(GetTestPremiumKey(_testSeasonData.seasonID), -1), "Premium PlayerPref should be cleared or 0.");
            Assert.AreEqual(0, PlayerPrefs.GetInt(GetTestClaimedKey(_testSeasonData.seasonID, 1, false, _xpReward.rewardID), -1), "Claimed PlayerPref should be cleared or 0.");
        }
    }
}
