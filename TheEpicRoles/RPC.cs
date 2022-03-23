using HarmonyLib;
using Hazel;
using static TheEpicRoles.TheEpicRoles;
using static TheEpicRoles.HudManagerStartPatch;
using static TheEpicRoles.GameHistory;
using static TheEpicRoles.MapOptions;
using TheEpicRoles.Objects;
using TheEpicRoles.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace TheEpicRoles {
    enum RoleId {
        Jester,
        Mayor,
        Engineer,
        Sheriff,
        Deputy,
        Lighter,
        Godfather,
        Mafioso,
        Janitor,
        Detective,
        TimeMaster,
        Medic,
        Shifter,
        Swapper,
        Lover,
        Seer,
        Morphling,
        Camouflager,
        Hacker,
        Mini,
        Tracker,
        Vampire,
        Snitch,
        Jackal,
        Sidekick,
        Eraser,
        Spy,
        Trickster,
        Cleaner,
        Warlock,
        SecurityGuard,
        Arsonist,
        EvilGuesser,
        NiceGuesser,
        BountyHunter,
        Bait,
        Vulture,
        Medium,
        Lawyer,
        Pursuer,
        Witch,
        Phaser,
        Crewmate,
        Impostor
    }

    enum CustomRPC {
        // Main Controls

        ResetVaribles = 60,
        ShareOptions,
        ForceEnd,
        SetRole,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,
        DynamicMapOption,

        // Role functionality

        EngineerFixLights = 91,
        EngineerUsedRepair,
        CleanBody,
        MedicSetShielded,
        ShieldedMurderAttempt,
        TimeMasterShield,
        TimeMasterRewindTime,
        ShifterShift,
        ShifterKilledDueBadShift,
        SwapperSwap,
        MorphlingMorph,
        CamouflagerCamouflage,
        TrackerUsedTracker,
        VampireSetBitten,
        PlaceGarlic,
        DeputyUsedHandcuffs,
        DeputyPromotes,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        SetFutureShielded,
        SetFutureSpelled,
        PlaceJackInTheBox,
        LightsOut,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot,
        VultureWin,
        LawyerWin,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        SetBlanked,
        ShieldedGuess,
        BlankFired,
        VentUsed,
        HandcuffStatus,
    }

    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            Garlic.clearGarlics();
            JackInTheBox.clearJackInTheBoxes();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
        }

        public static void ShareOptions(int numberOfOptions, MessageReader reader) {
            try {
                for (int i = 0; i < numberOfOptions; i++) {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e) {
                TheEpicRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void forceEnd() {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (!player.Data.Role.IsImpostor) {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void setRole(byte roleId, byte playerId, byte flag) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == playerId) {
                    switch ((RoleId)roleId) {
                        case RoleId.Jester:
                            Jester.jester = player;
                            break;
                        case RoleId.Mayor:
                            Mayor.mayor = player;
                            break;
                        case RoleId.Engineer:
                            Engineer.engineer = player;
                            break;
                        case RoleId.Sheriff:
                            Sheriff.sheriff = player;
                            break;
                        case RoleId.Deputy:
                            Deputy.deputy = player;
                            break;
                        case RoleId.Lighter:
                            Lighter.lighter = player;
                            break;
                        case RoleId.Godfather:
                            Godfather.godfather = player;
                            break;
                        case RoleId.Mafioso:
                            Mafioso.mafioso = player;
                            break;
                        case RoleId.Janitor:
                            Janitor.janitor = player;
                            break;
                        case RoleId.Detective:
                            Detective.detective = player;
                            break;
                        case RoleId.TimeMaster:
                            TimeMaster.timeMaster = player;
                            break;
                        case RoleId.Medic:
                            Medic.medic = player;
                            break;
                        case RoleId.Shifter:
                            Shifter.shifter = player;
                            break;
                        case RoleId.Swapper:
                            Swapper.swapper = player;
                            break;
                        case RoleId.Lover:
                            if (flag == 0) {
                                Lovers.lover1 = player;
                            }
                            else {
                                Lovers.lover2 = player;
                            }
                            break;
                        case RoleId.Seer:
                            Seer.seer = player;
                            break;
                        case RoleId.Morphling:
                            Morphling.morphling = player;
                            break;
                        case RoleId.Camouflager:
                            Camouflager.camouflager = player;
                            break;
                        case RoleId.Hacker:
                            Hacker.hacker = player;
                            break;
                        case RoleId.Mini:
                            Mini.mini = player;
                            break;
                        case RoleId.Tracker:
                            Tracker.tracker = player;
                            break;
                        case RoleId.Vampire:
                            Vampire.vampire = player;
                            break;
                        case RoleId.Snitch:
                            Snitch.snitch = player;
                            break;
                        case RoleId.Jackal:
                            Jackal.jackal = player;
                            break;
                        case RoleId.Sidekick:
                            Sidekick.sidekick = player;
                            break;
                        case RoleId.Eraser:
                            Eraser.eraser = player;
                            break;
                        case RoleId.Spy:
                            Spy.spy = player;
                            break;
                        case RoleId.Trickster:
                            Trickster.trickster = player;
                            break;
                        case RoleId.Cleaner:
                            Cleaner.cleaner = player;
                            break;
                        case RoleId.Warlock:
                            Warlock.warlock = player;
                            break;
                        case RoleId.SecurityGuard:
                            SecurityGuard.securityGuard = player;
                            break;
                        case RoleId.Arsonist:
                            Arsonist.arsonist = player;
                            break;
                        case RoleId.EvilGuesser:
                            Guesser.evilGuesser = player;
                            break;
                        case RoleId.NiceGuesser:
                            Guesser.niceGuesser = player;
                            break;
                        case RoleId.BountyHunter:
                            BountyHunter.bountyHunter = player;
                            break;
                        case RoleId.Bait:
                            Bait.bait = player;
                            break;
                        case RoleId.Vulture:
                            Vulture.vulture = player;
                            break;
                        case RoleId.Medium:
                            Medium.medium = player;
                            break;
                        case RoleId.Lawyer:
                            Lawyer.lawyer = player;
                            break;
                        case RoleId.Pursuer:
                            Pursuer.pursuer = player;
                            break;
                        case RoleId.Witch:
                            Witch.witch = player;
                            break;
                        case RoleId.Phaser:
                            Phaser.phaser = player;
                            break;
                    }
                    Log.add(Log.assignRole(RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == roleId).name), player, showCoords: false);
                }
        }
        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId) {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new MessageReader();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.startAnimation(ventId);
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
            if (isEnter == 0)
                Log.add(Log.tricksterIntoVent(ventId), player);
            else
                Log.add(Log.tricksterOutVent(ventId), player);
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation) {
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null) {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
                Log.add(Log.killed, source, target);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId) {
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null) {
                source.ReportDeadBody(target.Data);
                Log.add(Log.reportBody, source, target);
            }
        }

        public static void uncheckedExilePlayer(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null) {
                target.Exiled();
                Log.add(Log.exiled, target);
            }
        }

        public static void dynamicMapOption(byte mapId) {
            PlayerControl.GameOptions.MapId = mapId;
            Log.add(Log.map(mapId));
        }

        // Role functionality

        public static void engineerFixLights() {
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            Log.add(Log.engiFixSabotage);
        }

        public static void engineerUsedRepair() {
            Engineer.remainingFixes--;
            if (Engineer.remainingFixes == 0) {
                Log.add(Log.noFixesRemaining);
            }
        }

        public static void cleanBody(byte playerId) {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId) {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                    Log.add(Log.clean, Cleaner.cleaner, Helpers.playerById(playerId));
                }
            }
        }

        public static void timeMasterRewindTime() {
            Log.add(Log.rewindTime, TimeMaster.timeMaster);

            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer) {
                resetTimeMasterButton();
            }
            HudManager.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.Instance.FullScreen.enabled = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) => {
                if (p == 1f) HudManager.Instance.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null || PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void timeMasterShield() {
            TimeMaster.shieldActive = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>((p) => {
                if (p == 1f) TimeMaster.shieldActive = false;
            })));
        }

        public static void medicSetShielded(byte shieldedId) {
            Medic.usedShield = true;
            PlayerControl player = Helpers.playerById(shieldedId);
            Medic.shielded = player;
            Medic.futureShielded = null;
            Log.add(Log.shielded, player);
        }

        public static void shieldedMurderAttempt() {
            if (Medic.shielded == null || Medic.medic == null) return;

            bool isShieldedAndShow = Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
            bool isMedicAndShow = Medic.medic == PlayerControl.LocalPlayer && Medic.showAttemptToMedic;

            if (Medic.showAttemptToShielded) Log.add(Log.showAttemptToShielded, Medic.shielded);
            if (Medic.showAttemptToMedic) Log.add(Log.showAttemptToMedic, Medic.medic, Medic.shielded);

            if ((isShieldedAndShow || isMedicAndShow) && HudManager.Instance?.FullScreen != null) {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(0.5f, new Action<float>((p) => {
                    var renderer = HudManager.Instance.FullScreen;
                    Color c = Palette.ImpostorRed;
                    if (p < 0.5) {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }
        }

        public static void shifterShift(byte targetId) {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;
            Shifter.futureShift = null;
            string oldRole = RoleInfo.getRoleInfoForPlayer(player).FirstOrDefault()?.name;

            // Suicide (exile) when impostor or impostor variants
            if (Shifter.checkTargetIsBad(player)) {
                Log.add(Log.shiftedBad(oldRole), oldShifter, player);
                oldShifter.Exiled();
                return;
            }

            Log.add(Log.shifted(oldRole), oldShifter, player);
            Shifter.clearAndReload();

            if (Shifter.shiftModifiers) {
                // Switch shield
                if (Medic.shielded != null && Medic.shielded == player) {
                    Medic.shielded = oldShifter;
                    Log.add(Log.shifterStoleShield, oldShifter, player);
                }
                else if (Medic.shielded != null && Medic.shielded == oldShifter) {
                    Medic.shielded = player;
                    Log.add(Log.shifterGaveShield, oldShifter, player);
                }
                // Shift Lovers Role
                if (Lovers.lover1 != null && oldShifter == Lovers.lover1) Lovers.lover1 = player;
                else if (Lovers.lover1 != null && player == Lovers.lover1) Lovers.lover1 = oldShifter;

                if (Lovers.lover2 != null && oldShifter == Lovers.lover2) Lovers.lover2 = player;
                else if (Lovers.lover2 != null && player == Lovers.lover2) Lovers.lover2 = oldShifter;
            }

            // Shift role
            if (Mayor.mayor != null && Mayor.mayor == player)
                Mayor.mayor = oldShifter;
            if (Engineer.engineer != null && Engineer.engineer == player)
                Engineer.engineer = oldShifter;
            if (Sheriff.sheriff != null && Sheriff.sheriff == player) {
                if (Sheriff.formerDeputy != null && Sheriff.formerDeputy == Sheriff.sheriff) Sheriff.formerDeputy = oldShifter;  // Shifter also shifts info on promoted deputy (to get handcuffs)
                Sheriff.sheriff = oldShifter;
            }
            if (Deputy.deputy != null && Deputy.deputy == player)
                Deputy.deputy = oldShifter;
            if (Lighter.lighter != null && Lighter.lighter == player)
                Lighter.lighter = oldShifter;
            if (Detective.detective != null && Detective.detective == player)
                Detective.detective = oldShifter;
            if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == player)
                TimeMaster.timeMaster = oldShifter;
            if (Medic.medic != null && Medic.medic == player)
                Medic.medic = oldShifter;
            if (Swapper.swapper != null && Swapper.swapper == player)
                Swapper.swapper = oldShifter;
            if (Seer.seer != null && Seer.seer == player)
                Seer.seer = oldShifter;
            if (Hacker.hacker != null && Hacker.hacker == player)
                Hacker.hacker = oldShifter;
            if (Mini.mini != null && Mini.mini == player)
                Mini.mini = oldShifter;
            if (Tracker.tracker != null && Tracker.tracker == player)
                Tracker.tracker = oldShifter;
            if (Snitch.snitch != null && Snitch.snitch == player)
                Snitch.snitch = oldShifter;
            if (Spy.spy != null && Spy.spy == player)
                Spy.spy = oldShifter;
            if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == player)
                SecurityGuard.securityGuard = oldShifter;
            if (Guesser.niceGuesser != null && Guesser.niceGuesser == player)
                Guesser.niceGuesser = oldShifter;
            if (Bait.bait != null && Bait.bait == player) {
                Bait.bait = oldShifter;
                if (Bait.bait.Data.IsDead) Bait.reported = true;
            }
            if (Medium.medium != null && Medium.medium == player)
                Medium.medium = oldShifter;
            if (Phaser.phaser != null && Phaser.phaser == player)
                Phaser.phaser = oldShifter;

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player) {
                CustomButton.ResetAllCooldowns();
            }
            if (Shifter.shiftSelf) {
                Shifter.shifter = player;
                Log.add(Log.newShifter, player);
            }
        }
        public static void shifterKilledDueBadShift() {
            if (Shifter.shifter != null) {
                Shifter.shiftedBadRole = true;
            }
        }

        public static void swapperSwap(byte playerId1, byte playerId2) {
            if (MeetingHud.Instance) {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
                Log.add(Log.swappedPlayer, Helpers.playerById(playerId1), Helpers.playerById(playerId2));
            }
        }

        public static void morphlingMorph(byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;

            Log.add(Log.morphes, Morphling.morphling, target);
            Morphling.morphTimer = Morphling.duration;
            Morphling.morphTarget = target;
            if (Camouflager.camouflageTimer <= 0f)
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void camouflagerCamouflage() {
            if (Camouflager.camouflager == null) return;

            Log.add(Log.camouflageActive, Camouflager.camouflager);
            Camouflager.camouflageTimer = Camouflager.duration;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.setLook("", 6, "", "", "", "");
        }

        public static void vampireSetBitten(byte targetId, byte performReset) {
            if (performReset != 0) {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.vampire == null) return;

            PlayerControl target = Helpers.playerById(targetId);
            if (target != null && !target.Data.IsDead) {
                Vampire.bitten = target;
                Log.add(Log.bitten, Vampire.vampire, target);
            }
        }

        public static void placeGarlic(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new Garlic(position);
            Log.add(Log.garlicAdded(position), showCoords: false);
        }

        public static void trackerUsedTracker(byte targetId) {
            Tracker.usedTracker = true;
            PlayerControl player = Helpers.playerById(targetId);
            if (player != null) {
                Tracker.tracked = player;
                Log.add(Log.tracked, Tracker.tracker, player);
            }
        }

        public static void deputyUsedHandcuffs(byte targetId) {
            Deputy.remainingHandcuffs--;
            Deputy.handcuffedPlayers.Add(targetId);
            Log.add(Log.addedHandcuffs, Deputy.deputy, Helpers.playerById(targetId));
            if (Deputy.remainingHandcuffs == 0) Log.add(Log.lastHandcuffUsed, Deputy.deputy);
        }

        public static void deputyPromotes() {
            if (Deputy.deputy != null) {  // Deputy should never be null here, but there appeared to be a race condition during testing, which was removed.
                Log.add(Log.deputyPromote, Deputy.deputy, Sheriff.sheriff); // Not sure if Sheriff.sheriff could be null, but doesn't matter.
                Sheriff.replaceCurrentSheriff(Deputy.deputy);
                Sheriff.formerDeputy = Deputy.deputy;
                Deputy.deputy = null;
                // No clear and reload, as we need to keep the number of handcuffs left etc
            }
        }

        public static void jackalCreatesSidekick(byte targetId) {
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null) return;

            if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor) {
                Jackal.fakeSidekick = player;
                Log.add(Log.fakeSidekick, Jackal.jackal, player);
            }
            else {
                Log.add(Log.createSidekick, Jackal.jackal, player);
                DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                erasePlayerRoles(player.PlayerId, true);
                Sidekick.sidekick = player;
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
            }
            Jackal.canCreateSidekick = false;
        }

        public static void sidekickPromotes() {
            Log.add(Log.sidekickPromote, Sidekick.sidekick, Jackal.jackal);
            Jackal.removeCurrentJackal();
            Jackal.jackal = Sidekick.sidekick;
            Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
            Sidekick.clearAndReload();
            return;
        }

        public static void erasePlayerRoles(byte playerId, bool ignoreLovers = false) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;

            // Crewmate roles
            if (player == Mayor.mayor) {
                Mayor.clearAndReload();
                Log.add(Log.eraseRole("Mayor"), player);
            }
            if (player == Engineer.engineer) {
                Engineer.clearAndReload();
                Log.add(Log.eraseRole("Engineer"), player);
            }
            if (player == Sheriff.sheriff) {
                Sheriff.clearAndReload();
                Log.add(Log.eraseRole("Sheriff"), player);
            }
            if (player == Deputy.deputy) {
                Deputy.clearAndReload();
                Log.add(Log.eraseRole("Deputy"), player);
            }
            if (player == Lighter.lighter) {
                Lighter.clearAndReload();
                Log.add(Log.eraseRole("Lighter"), player);
            }
            if (player == Detective.detective) {
                Detective.clearAndReload();
                Log.add(Log.eraseRole("Detective"), player);
            }
            if (player == TimeMaster.timeMaster) {
                TimeMaster.clearAndReload();
                Log.add(Log.eraseRole("Time Master"), player);
            }
            if (player == Medic.medic) {
                Medic.clearAndReload();
                Log.add(Log.eraseRole("Medic"), player);
            }
            if (player == Shifter.shifter) {
                Shifter.clearAndReload();
                Log.add(Log.eraseRole("Shifter"), player);
            }
            if (player == Seer.seer) {
                Seer.clearAndReload();
                Log.add(Log.eraseRole("Seer"), player);
            }
            if (player == Hacker.hacker) {
                Hacker.clearAndReload();
                Log.add(Log.eraseRole("Hacker"), player);
            }
            if (player == Mini.mini) {
                Mini.clearAndReload();
                Log.add(Log.eraseRole("Mini"), player);
            }
            if (player == Tracker.tracker) {
                Tracker.clearAndReload();
                Log.add(Log.eraseRole("Tracker"), player);
            }
            if (player == Snitch.snitch) {
                Snitch.clearAndReload();
                Log.add(Log.eraseRole("Snitch"), player);
            }
            if (player == Swapper.swapper) {
                Swapper.clearAndReload();
                Log.add(Log.eraseRole("Swapper"), player);
            }
            if (player == Spy.spy) {
                Spy.clearAndReload();
                Log.add(Log.eraseRole("Spy"), player);
            }
            if (player == SecurityGuard.securityGuard) {
                SecurityGuard.clearAndReload();
                Log.add(Log.eraseRole("Security Guard"), player);
            }
            if (player == Bait.bait) {
                Bait.clearAndReload();
                Log.add(Log.eraseRole("Bait"), player);
            }
            if (player == Medium.medium) {
                Medium.clearAndReload();
                Log.add(Log.eraseRole("Medium"), player);
            }

            // Impostor roles
            if (player == Morphling.morphling) {
                Morphling.clearAndReload();
                Log.add(Log.eraseRole("Morphling"), player);
            }
            if (player == Camouflager.camouflager) {
                Camouflager.clearAndReload();
                Log.add(Log.eraseRole("Camouflager"), player);
            }
            if (player == Godfather.godfather) {
                Godfather.clearAndReload();
                Log.add(Log.eraseRole("Godfather"), player);
            }
            if (player == Mafioso.mafioso) {
                Mafioso.clearAndReload();
                Log.add(Log.eraseRole("Mafioso"), player);
            }
            if (player == Janitor.janitor) {
                Janitor.clearAndReload();
                Log.add(Log.eraseRole("Janitor"), player);
            }
            if (player == Vampire.vampire) {
                Vampire.clearAndReload();
                Log.add(Log.eraseRole("Vampire"), player);
            }
            if (player == Eraser.eraser) {
                Eraser.clearAndReload();
                Log.add(Log.eraseRole("Eraser"), player);
            }
            if (player == Trickster.trickster) {
                Trickster.clearAndReload();
                Log.add(Log.eraseRole("Trickster"), player);
            }
            if (player == Cleaner.cleaner) {
                Cleaner.clearAndReload();
                Log.add(Log.eraseRole("Cleaner"), player);
            }
            if (player == Warlock.warlock) {
                Warlock.clearAndReload();
                Log.add(Log.eraseRole("Warlock"), player);
            }
            if (player == Witch.witch) {
                Witch.clearAndReload();
                Log.add(Log.eraseRole("Witch"), player);
            }
            if (player == Phaser.phaser) {
                Phaser.clearAndReload();
                Log.add(Log.eraseRole("Phaser"), player);
            }

            // Other roles
            if (player == Jester.jester) {
                Jester.clearAndReload();
                Log.add(Log.eraseRole("Jester"), player);
            }
            if (player == Arsonist.arsonist) {
                Arsonist.clearAndReload();
                Log.add(Log.eraseRole("Arsonist"), player);
            }
            if (!ignoreLovers && (player == Lovers.lover1 || player == Lovers.lover2)) { // The whole Lover couple is being erased
                Lovers.clearAndReload();
                Log.add(Log.eraseRole("Lover"), player);
            }
            if (Guesser.isGuesser(player.PlayerId)) {
                if (Guesser.niceGuesser == player)
                    Log.add(Log.eraseRole("Nice Guesser"), player);
                else
                    Log.add(Log.eraseRole("Evil Guesser"), player);
                Guesser.clear(player.PlayerId);
            }
            if (player == Jackal.jackal) { // Promote Sidekick and hence override the the Jackal or erase Jackal
                Log.add(Log.eraseRole("Jackal"), player);
                if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead) {
                    RPCProcedure.sidekickPromotes();
                }
                else {
                    Jackal.clearAndReload();
                }
            }
            if (player == Sidekick.sidekick) {
                Sidekick.clearAndReload();
                Log.add(Log.eraseRole("Sidekick"), player);
            }
            if (player == BountyHunter.bountyHunter) {
                BountyHunter.clearAndReload();
                Log.add(Log.eraseRole("BountyHunter"), player);
            }
            if (player == Vulture.vulture) {
                Vulture.clearAndReload();
                Log.add(Log.eraseRole("Vulture"), player);
            }
            if (player == Lawyer.lawyer) {
                Lawyer.clearAndReload();
                Log.add(Log.eraseRole("Lawyer"), player);
            }
            if (player == Pursuer.pursuer) {
                Pursuer.clearAndReload();
                Log.add(Log.eraseRole("Pursuer"), player);
            }
        }

        public static void setFutureErased(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (Eraser.futureErased == null)
                Eraser.futureErased = new List<PlayerControl>();
            if (player != null) {
                Eraser.futureErased.Add(player);
                Log.add(Log.markToBeErased, Eraser.eraser, player);
            }
        }

        public static void setFutureShifted(byte playerId) {
            Shifter.futureShift = Helpers.playerById(playerId);
            Log.add(Log.markToBeShifted, Shifter.shifter, Shifter.futureShift);
        }

        public static void setFutureShielded(byte playerId) {
            Medic.futureShielded = Helpers.playerById(playerId);
            Medic.usedShield = true;
            Log.add(Log.markToBeShielded, Medic.medic, Medic.futureShielded);
        }

        public static void setFutureSpelled(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (Witch.futureSpelled == null)
                Witch.futureSpelled = new List<PlayerControl>();
            if (player != null) {
                Witch.futureSpelled.Add(player);
                Log.add(Log.addSpelled, Witch.witch, player);
            }
        }

        public static void placeJackInTheBox(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new JackInTheBox(position);
            Log.add(Log.placeJackInTheBox(position), Trickster.trickster, showCoords: false);
        }

        public static void lightsOut() {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                new CustomMessage("Lights are out", Trickster.lightsOutDuration);
            }
            Log.add(Log.tricksterLightOut, Trickster.trickster);
        }

        public static void placeCamera(byte[] buff) {
            var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));

            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
            if (PlayerControl.GameOptions.MapId == 2 || PlayerControl.GameOptions.MapId == 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 

            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard) {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else {
                camera.gameObject.SetActive(false);
            }
            MapOptions.camerasToAdd.Add(camera);
            Log.add(Log.addCamera(position), SecurityGuard.securityGuard);
        }

        public static void sealVent(int ventId) {
            Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            MapOptions.ventsToSeal.Add(vent);
            Log.add(Log.sealVent(vent), SecurityGuard.securityGuard);
        }

        public static void arsonistWin() {
            Arsonist.triggerArsonistWin = true;
            Log.add(Log.arsonistIgnite, Arsonist.arsonist);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p != Arsonist.arsonist) p.Exiled();
            }
        }

        public static void vultureWin() {
            Vulture.triggerVultureWin = true;
            Log.add(Log.vultureLastBodyEaten, Vulture.vulture);
        }

        public static void lawyerWin() {
            Lawyer.triggerLawyerWin = true;
            Log.add(Log.lawyerMeetingWin, Lawyer.lawyer);
        }

        public static void lawyerSetTarget(byte playerId) {
            Lawyer.target = Helpers.playerById(playerId);
            Log.add(Log.lawyerTarget, Lawyer.lawyer, Lawyer.target);
        }

        public static void lawyerPromotesToPursuer() {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;

            // Don't promote to pursuer if target is jester and the jester was exiled in the meeting
            if (client != null && client == Jester.jester && Jester.meetingExile == true) {
                Jester.meetingExile = false;
                return;
            }
            Log.add(Log.lawyerPromotesToPursuer, player, client);
            Lawyer.clearAndReload();
            Pursuer.pursuer = player;

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null) {
                Transform playerInfoTransform = client.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId) {
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;

            string roleName = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId).name;
            if (dyingTargetId == guessedTargetId)
                Log.add(Log.guessWrong(roleName), Helpers.playerById(killerId), Helpers.playerById(guessedTargetId));
            else
                Log.add(Log.guessRight(roleName), Helpers.playerById(killerId), Helpers.playerById(guessedTargetId));

            dyingTarget.Exiled();
            PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check
            byte partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

            Guesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (MeetingHud.Instance) {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates) {
                    if (pva.TargetPlayerId == dyingTargetId || pva.TargetPlayerId == partnerId) {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }

                    //Give players back their vote if target is shot dead
                    if (pva.VotedFor != dyingTargetId || pva.VotedFor != partnerId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();

                }
                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
            PlayerControl guesser = Helpers.playerById(killerId);
            if (HudManager.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);

            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (Guesser.showInfoInGhostChat && PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null) {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                string msg = $"Guesser guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
                if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    DestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
            }
        }

        public static void shieldedGuess(byte playerId, byte targetId, byte roleId) {
            PlayerControl player = Helpers.playerById(playerId);
            PlayerControl target = Helpers.playerById(targetId);
            string roleName = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == roleId).name;

            Log.add(Log.shieldedGuess(roleName), player, target, false);
            RPCProcedure.shieldedMurderAttempt();
        }

        public static void setBlanked(byte playerId, byte value) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) {
                Pursuer.blankedList.Add(target);
                Log.add(Log.injectBlank, Pursuer.pursuer, target);
            } // Log for firing a blank is in CheckMurderAttempt
        }

        public static void blankFired(byte playerId, byte target) {
            Log.add(Log.firedBlank, Helpers.playerById(playerId), Helpers.playerById(target));
        }

        public static void ventUsed(byte playerId, int ventId, byte isEnter) {
            Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            Log.add(Log.useVent(vent, isEnter != 0), Helpers.playerById(playerId));
        }
        
        public static void handcuffStatus(byte playerId, byte handcuffStatus) {
            PlayerControl player = Helpers.playerById(playerId);
            if (handcuffStatus == 0)
                Log.add(Log.handcuffTimedOut, player);
            else
                Log.add(Log.handcuffActivated, player);
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader) {
                byte packetId = callId;
                switch (packetId) {

                    // Main Controls

                    case (byte)CustomRPC.ResetVaribles:
                        RPCProcedure.resetVariables();
                        break;
                    case (byte)CustomRPC.ShareOptions:
                        RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                        break;
                    case (byte)CustomRPC.ForceEnd:
                        RPCProcedure.forceEnd();
                        break;
                    case (byte)CustomRPC.SetRole:
                        byte roleId = reader.ReadByte();
                        byte playerId = reader.ReadByte();
                        byte flag = reader.ReadByte();
                        RPCProcedure.setRole(roleId, playerId, flag);
                        break;
                    case (byte)CustomRPC.VersionHandshake:
                        byte major = reader.ReadByte();
                        byte minor = reader.ReadByte();
                        byte patch = reader.ReadByte();
                        int versionOwnerId = reader.ReadPackedInt32();
                        byte revision = 0xFF;
                        Guid guid;
                        if (reader.Length - reader.Position >= 17) { // enough bytes left to read
                            revision = reader.ReadByte();
                            // GUID
                            byte[] gbytes = reader.ReadBytes(16);
                            guid = new Guid(gbytes);
                        }
                        else {
                            guid = new Guid(new byte[16]);
                        }
                        RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                        break;
                    case (byte)CustomRPC.UseUncheckedVent:
                        int ventId = reader.ReadPackedInt32();
                        byte ventingPlayer = reader.ReadByte();
                        byte isEnter = reader.ReadByte();
                        RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                        break;
                    case (byte)CustomRPC.UncheckedMurderPlayer:
                        byte source = reader.ReadByte();
                        byte target = reader.ReadByte();
                        byte showAnimation = reader.ReadByte();
                        RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                        break;
                    case (byte)CustomRPC.UncheckedExilePlayer:
                        byte exileTarget = reader.ReadByte();
                        RPCProcedure.uncheckedExilePlayer(exileTarget);
                        break;
                    case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                        byte reportSource = reader.ReadByte();
                        byte reportTarget = reader.ReadByte();
                        RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                        break;
                    case (byte)CustomRPC.DynamicMapOption:
                        byte mapId = reader.ReadByte();
                        RPCProcedure.dynamicMapOption(mapId);
                        break;

                    // Role functionality

                    case (byte)CustomRPC.EngineerFixLights:
                        RPCProcedure.engineerFixLights();
                        break;
                    case (byte)CustomRPC.EngineerUsedRepair:
                        RPCProcedure.engineerUsedRepair();
                        break;
                    case (byte)CustomRPC.CleanBody:
                        RPCProcedure.cleanBody(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.TimeMasterRewindTime:
                        RPCProcedure.timeMasterRewindTime();
                        break;
                    case (byte)CustomRPC.TimeMasterShield:
                        RPCProcedure.timeMasterShield();
                        break;
                    case (byte)CustomRPC.MedicSetShielded:
                        RPCProcedure.medicSetShielded(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ShieldedMurderAttempt:
                        RPCProcedure.shieldedMurderAttempt();
                        break;
                    case (byte)CustomRPC.ShifterShift:
                        RPCProcedure.shifterShift(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ShifterKilledDueBadShift:
                        RPCProcedure.shifterKilledDueBadShift();
                        break;
                    case (byte)CustomRPC.VampireSetBitten:
                        byte bittenId = reader.ReadByte();
                        byte reset = reader.ReadByte();
                        RPCProcedure.vampireSetBitten(bittenId, reset);
                        break;
                    case (byte)CustomRPC.SwapperSwap:
                        byte playerId1 = reader.ReadByte();
                        byte playerId2 = reader.ReadByte();
                        RPCProcedure.swapperSwap(playerId1, playerId2);
                        break;
                    case (byte)CustomRPC.MorphlingMorph:
                        RPCProcedure.morphlingMorph(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.CamouflagerCamouflage:
                        RPCProcedure.camouflagerCamouflage();
                        break;
                    case (byte)CustomRPC.PlaceGarlic:
                        RPCProcedure.placeGarlic(reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRPC.TrackerUsedTracker:
                        RPCProcedure.trackerUsedTracker(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.DeputyUsedHandcuffs:
                        RPCProcedure.deputyUsedHandcuffs(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.DeputyPromotes:
                        RPCProcedure.deputyPromotes();
                        break;
                    case (byte)CustomRPC.JackalCreatesSidekick:
                        RPCProcedure.jackalCreatesSidekick(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SidekickPromotes:
                        RPCProcedure.sidekickPromotes();
                        break;
                    case (byte)CustomRPC.ErasePlayerRoles:
                        RPCProcedure.erasePlayerRoles(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetFutureErased:
                        RPCProcedure.setFutureErased(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetFutureShifted:
                        RPCProcedure.setFutureShifted(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetFutureShielded:
                        RPCProcedure.setFutureShielded(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.PlaceJackInTheBox:
                        RPCProcedure.placeJackInTheBox(reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRPC.LightsOut:
                        RPCProcedure.lightsOut();
                        break;
                    case (byte)CustomRPC.PlaceCamera:
                        RPCProcedure.placeCamera(reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRPC.SealVent:
                        RPCProcedure.sealVent(reader.ReadPackedInt32());
                        break;
                    case (byte)CustomRPC.ArsonistWin:
                        RPCProcedure.arsonistWin();
                        break;
                    case (byte)CustomRPC.GuesserShoot:
                        byte killerId = reader.ReadByte();
                        byte dyingTarget = reader.ReadByte();
                        byte guessedTarget = reader.ReadByte();
                        byte guessedRoleId = reader.ReadByte();
                        RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                        break;
                    case (byte)CustomRPC.VultureWin:
                        RPCProcedure.vultureWin();
                        break;
                    case (byte)CustomRPC.LawyerWin:
                        RPCProcedure.lawyerWin();
                        break;
                    case (byte)CustomRPC.LawyerSetTarget:
                        RPCProcedure.lawyerSetTarget(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.LawyerPromotesToPursuer:
                        RPCProcedure.lawyerPromotesToPursuer();
                        break;
                    case (byte)CustomRPC.SetBlanked:
                        var pid = reader.ReadByte();
                        var blankedValue = reader.ReadByte();
                        RPCProcedure.setBlanked(pid, blankedValue);
                        break;
                    case (byte)CustomRPC.SetFutureSpelled:
                        RPCProcedure.setFutureSpelled(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ShieldedGuess:
                        playerId = reader.ReadByte();
                        byte targetId = reader.ReadByte();
                        roleId = reader.ReadByte();
                        RPCProcedure.shieldedGuess(playerId, targetId, roleId);
                        break;
                    case (byte)CustomRPC.BlankFired:
                        playerId = reader.ReadByte();
                        targetId = reader.ReadByte();
                        RPCProcedure.blankFired(playerId, targetId);
                        break;
                    case (byte)CustomRPC.VentUsed:
                        ventingPlayer = reader.ReadByte();
                        ventId = reader.ReadPackedInt32();
                        isEnter = reader.ReadByte();
                        RPCProcedure.ventUsed(ventingPlayer, ventId, isEnter);
                        break;
                    case (byte)CustomRPC.HandcuffStatus:
                        playerId = reader.ReadByte();
                        byte handcuffStatus = reader.ReadByte();
                        RPCProcedure.handcuffStatus(playerId, handcuffStatus);
                        break;
                }
            }
        }
    }
}