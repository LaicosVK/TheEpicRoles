using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TheEpicRoles;
using UnityEngine;

// Log class to create txt log files for all games and important events like kills, given medic shield, shifts, ...
public static class Log {
    public const string logPath = "log/";
    
    private static DateTime gameStartTime;
    private static List<string> log = new List<string>();

    // Log line general format:
    //    [time] Player1 Event Player2 CoordinatesOfPlayer1
    // Log line example:
    //    [20:23:11] LVK tried to kill shielded Nova (2.873, -1.203)
    public static void add(string logEvent, PlayerControl player1 = null, PlayerControl player2 = null, bool showCoords = true) {
        string logLine = "";
        if (logEvent != null && !logEvent.Equals(""))
            logLine = DateTime.Now.ToString("[HH:mm:ss]");
        if (player1 != null)
            logLine += " " + player1.Data.PlayerName;
        if (logEvent != null)
            logLine += " " + logEvent;
        if (player2 != null)
            logLine += " " + player2.Data.PlayerName;
        if (player1 != null && showCoords)
            logLine += " " + positionString(player1.transform.position);
        log.Add(logLine);
    }
    public static void startLogging() {
        gameStartTime = DateTime.Now;
        add(Log.gameStart);
    }
    public static void finishLogging() {
        add(gameEnd);
        string fileNamePrefix = logPath + formatTime(gameStartTime);
        string fileFullName = fileNamePrefix + ".log";
        int gameLogId = 0;

        // Try file names until one is found that doesn't already exist. First try without ID, then with incrementing ID in brackets.
        while (File.Exists(fileFullName) && gameLogId < 100) {
            gameLogId++;
            fileFullName = fileNamePrefix + " (" + gameLogId + ").log"; 
        }

        if (gameStartTime == null || gameStartTime == default(DateTime)) {
            File.WriteAllTextAsync(logPath + "error " + formatTime(DateTime.Now) + " " + TheEpicRoles.TheEpicRoles.rnd.Next() + ".log", "Error, Log was not properly started.");
        }
        else {
            File.WriteAllLinesAsync(fileFullName, log);
            gameStartTime = DateTime.Now;
        }
        log.Clear();
    }

    // Basic log messages
    public static readonly string
        gameStart = "#### GAME START ####" + Environment.NewLine,
        gameEnd = "#### GAME END ####",
        meetingStart = "has started a meeting",
        meetingEnd = "#### MEETING END ####" + Environment.NewLine,
        reportBody = "has reported the body of",
        exiled = "was exiled",
        killed = "has killed",
        sabotageReactor = "did sabotage the reactor",
        sabotageLight = "did sabotage the lights",
        sabotageComs = "did sabotage the communication systems",
        sabotageO2 = "did sabotage the O2 system",
        sabotageSeismic = "did sabotage the seismic stabilisers",
        shielded = "shielded",
        shieldKillAttempt = "tried to kill shielded",
        newShifter = "is now the new Shifter",
        shifterStoleShield = "stole the shield from",
        shifterGaveShield = "gave his shield to",
        engiFixSabotage = "used an Engineer fix on the sabotage",
        noFixesRemaining = "has no Engineer fix charges remaining",
        activateTimeShield = "has activated the Time Shield",
        rewindTime = "bend the space–time continuum to rewind time itself",
        showAttemptToShielded = "saw an attempt to kill him",
        showAttemptToMedic = "saw an attempt to kill the shielded",
        morphes = "morphes into the look of",
        morphExpired = "Morph expired",
        camouflageActive = "activated the camouflage ability - gray blobs everywhere!",
        camouflageExpired = "Camouflage expired",
        bitten = "has bitten",
        killByBite = "the Vampire sucked all blood ouf of",
        tracked = "started tracking",
        addedHandcuffs = "has added handcuffs to",
        lastHandcuffUsed = "has used up his last handcuffs",
        deputyPromote = "is the successor of the old Sheriff",
        fakeSidekick = "tried to get a sidekick, but was tricked by Impostor",
        createSidekick = "was very convincing and is now the master of the new Sidekick",
        sidekickPromote = "was promoted and will take revenge for former Jackal",
        markToBeErased = "wants to erase the role of",
        markToBeShifted = "wants to shift",
        markToBeShielded = "wants to shield",
        addSpelled = "spoke a spell of doom over",
        spellStarted = "has started a spell on",
        tricksterLightOut = "is a sneaky trickster and put the lights out",
        arsonistDouseStart = "has opened their fuel canister and started dousing",
        arsonistDoused = "has finished dousing",
        arsonistIgnite = "has ignited the holy flame of destruction",
        vultureLastBodyEaten = "has devoured the last course and is now fed",
        lawyerMeetingWin = "was acquitted of all charges and won the lawsuit",
        lawyerTarget = "will defend the new client",
        lawyerPromotesToPursuer = "becomes the Pursuer after resigning as Lawyer for",
        injectBlank = "has blanked",
        firedBlank = "fired a blank at",
        handcuffActivated = "was handicapped by handcuffs",
        handcuffTimedOut = "got free from the Handcuffs",
        loverPartnerKill = "was heartbroken and ended themself",
        baitFlash = "the Bait showed their killer a flash of the trap they activated",
        seerFlash = "the Seer saw the last breath of a poor soul",
        morphlingSample = "sampled the DNA of",
        hackerAdminTable = "the Hacker took a look at his mobile Admin Table",
        hackerVitals = "the Hacker took a look at his mobile Vitals",
        hackerAbility = "activated his 1337 h4x0r ability",
        snitchRevealSelf = "was revealed as the Snitch to the forces of evil",
        snitchRevealEvil = "has snitched on a member of the forces of evil:", // LogToDo: no idea why, but this activates at the start of a round. pls fix
        bodyCleaned = "got their body cleaned up",
        cleanerCleaned = "the Cleaner removed the body of",
        janitorCleaned = "the Janitor cleaned the body of",
        vultureEaten = "the Vulture ate the body of",
        warlockCursed = "the Warlock has cursed",
        warlockCurseKill = "the Warlock has activated the Curse on",
        phaserMark = "the Phaser marked their target",
        phase = "phased to their target",
        secGuardRemote = "the Security Guard takes a look at their mobile device",
        bountyKilled = "has killed their Bounty",
        bountyMissed = "has missed their Bounty and instead killed",
        newBounty = "got a new Bounty:",

        newLine = "";

    // Comments with LogToDo are currently missing logging behaviours, mostly RPCs so everyone can log the event.
    // LogToDo: Task started, Task finished, all tasks done
    // LogToDo: Sabotage started, sabotage fixed, impo win sabotage

    // Special string functions for more advanced log messages
    public static string assignRole(string role) {
        return "was assigned the role " + role;
    }
    public static string tricksterIntoVent(int ventId) {

        return "entered Jack-in-the-box #" + ventId;
    }
    public static string tricksterOutVent(int ventId) {

        return "came out of Jack-in-the-box #" + ventId;
    }
    public static string map(byte mapId) {
        return "Map is " + Helpers.getMapName(mapId);
    }
    public static string garlicAdded(Vector3 position) {
        return "Garlic was placed at " + positionString(position); // LogToDo: add who placed the garlic
    }
    public static string eraseRole(string role) {
        return "lost the role " + role;
    }
    public static string placeJackInTheBox(Vector3 position) {
        return "has placed a Jack-in-the-box at " + positionString(position);
    }
    public static string addCamera(Vector3 position) {
        return "has placed a Camera at " + positionString(position);
    }
    public static string sealVent(Vent vent) {
        return "has sealed Vent " + vent.name + ", ID " + vent.Id;
    }
    public static string shieldedGuess(string roleName) {
        return "tried to guess " + roleName + " for the shielded player";
    }
    public static string guessWrong(string roleName) {
        return "wrongly guessed " + roleName + " for";
    }
    public static string guessRight(string roleName) {
        return "correctly guessed " + roleName + " for";
    }
    public static string shiftedBad(string targetRole) {
        return "made a bad choice and tried to shift the " + targetRole;
    }
    public static string shifted(string targetRole) {
        return "shifted the " + targetRole;
    }
    public static string useVent(Vent vent, bool isEnter) {
        return "has " + (isEnter ? "entered" : "left") + " the Vent " + vent.name + ", ID " + vent.Id;
    }
    public static string swapp(PlayerControl player1, PlayerControl player2) {
        return "swapped the votes of " + player1.Data.PlayerName + " and " + player2.Data.PlayerName;
    }
    public static string votes(int votes, string voterNames) {
        return "got " + votes + " vote" + (votes == 1 ? "" : "s") + (votes > 0 ? " (" + voterNames + ")" : "");
    }

    // Help functions for formatting
    public static string positionString(Vector3 position) {
        return "(" + position.x.ToString("F2", CultureInfo.InvariantCulture) + ", " + position.y.ToString("F2", CultureInfo.InvariantCulture) + ")";
    }
    public static string formatTime(DateTime time) {
        return time.ToString("yyyy-MM-dd - HH-mm-ss");
    }
}
