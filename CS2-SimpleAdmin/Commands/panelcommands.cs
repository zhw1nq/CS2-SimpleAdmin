using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using System.Linq;
using System.Threading.Tasks;
using CS2_SimpleAdminApi;
using System;

namespace CS2_SimpleAdmin;

public partial class CS2_SimpleAdmin
{
    [CommandHelper(minArgs: 5, usage: "<steamid> <admin_steamid> <admin_name> <time in minutes/0 perm> <reason>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnWebBanCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (DatabaseProvider == null) return;

        if (!Helper.ValidateSteamId(command.GetArg(1), out var steamId) || steamId == null)
        {
            command.ReplyToCommand("Invalid Target SteamID64.");
            return;
        }

        if (!ulong.TryParse(command.GetArg(2), out var adminSteamId))
        {
            command.ReplyToCommand("Invalid Admin SteamID64.");
            return;
        }

        string adminName = command.GetArg(3);
        string webAdminName = $"[WEBBAN] {adminName}";
        var time = Math.Max(0, Helper.ParsePenaltyTime(command.GetArg(4)));
        var reason = command.ArgCount >= 6
            ? string.Join(" ", Enumerable.Range(5, command.ArgCount - 5).Select(command.GetArg)).Trim()
            : _localizer?["sa_unknown"] ?? "Unknown";
        reason = string.IsNullOrWhiteSpace(reason) ? _localizer?["sa_unknown"] ?? "Unknown" : reason;

        var adminInfo = PlayersInfo.TryGetValue(adminSteamId, out var info)
            ? info
            : new PlayerInfo(null, 0, new SteamID(adminSteamId), adminName, null, 0, 0, 0, 0, 0);

        Task.Run(async () =>
        {
            await Server.NextWorldUpdateAsync(() =>
            {
                var player = Helper.GetPlayerFromSteamid64(steamId.SteamId64);
                if (player != null && player.IsValid)
                {
                    Ban(null, player, time, reason, webAdminName, silent: true, overrideAdminInfo: adminInfo);
                    return;
                }

                Task.Run(async () =>
                {
                    int? penaltyId = await BanManager.AddBanBySteamid(steamId.SteamId64, adminInfo, reason, time);
                    Helper.SendDiscordPenaltyMessage(null, steamId.SteamId64.ToString(), reason, time, PenaltyType.Ban, _localizer);

                    var activityArgs = time == 0
                        ? new object[] { webAdminName, steamId.SteamId64.ToString(), reason }
                        : new object[] { webAdminName, steamId.SteamId64.ToString(), reason, time };
                    var activityKey = time == 0 ? "sa_admin_ban_message_perm" : "sa_admin_ban_message_time";

                    await Server.NextWorldUpdateAsync(() =>
                    {
                        SimpleAdminApi?.OnPlayerPenaltiedAddedEvent(steamId, adminInfo, PenaltyType.Ban, reason, time, penaltyId);
                        Helper.ShowAdminActivity(activityKey, webAdminName, false, activityArgs);
                    });
                });
            });
        });
    }

    [CommandHelper(minArgs: 5, usage: "<steamid> <admin_steamid> <admin_name> <time in minutes/0 perm> <reason>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnWebMuteCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (DatabaseProvider == null) return;

        if (!Helper.ValidateSteamId(command.GetArg(1), out var steamId) || steamId == null) return;
        if (!ulong.TryParse(command.GetArg(2), out var adminSteamId)) return;

        string adminName = command.GetArg(3);
        string webAdminName = $"[WEBMUTE] {adminName}";
        var time = Math.Max(0, Helper.ParsePenaltyTime(command.GetArg(4)));
        var reason = command.ArgCount >= 6
            ? string.Join(" ", Enumerable.Range(5, command.ArgCount - 5).Select(command.GetArg)).Trim()
            : _localizer?["sa_unknown"] ?? "Unknown";
        reason = string.IsNullOrWhiteSpace(reason) ? _localizer?["sa_unknown"] ?? "Unknown" : reason;

        var adminInfo = PlayersInfo.TryGetValue(adminSteamId, out var info)
            ? info
            : new PlayerInfo(null, 0, new SteamID(adminSteamId), adminName, null, 0, 0, 0, 0, 0);

        Task.Run(async () =>
        {
            await Server.NextWorldUpdateAsync(() =>
            {
                var player = Helper.GetPlayerFromSteamid64(steamId.SteamId64);
                if (player != null && player.IsValid)
                {
                    Mute(null, player, time, reason, webAdminName, silent: true, overrideAdminInfo: adminInfo);
                    return;
                }

                Task.Run(async () =>
                {
                    int? penaltyId = await MuteManager.AddMuteBySteamid(steamId.SteamId64, adminInfo, reason, time, 1);
                    Helper.SendDiscordPenaltyMessage(null, steamId.SteamId64.ToString(), reason, time, PenaltyType.Mute, _localizer);

                    var activityArgs = time == 0
                        ? new object[] { webAdminName, steamId.SteamId64.ToString(), reason }
                        : new object[] { webAdminName, steamId.SteamId64.ToString(), reason, time };
                    var activityKey = time == 0 ? "sa_admin_mute_message_perm" : "sa_admin_mute_message_time";

                    await Server.NextWorldUpdateAsync(() =>
                    {
                        SimpleAdminApi?.OnPlayerPenaltiedAddedEvent(steamId, adminInfo, PenaltyType.Mute, reason, time, penaltyId);
                        Helper.ShowAdminActivity(activityKey, webAdminName, false, activityArgs);
                    });
                });
            });
        });
    }

    [CommandHelper(minArgs: 5, usage: "<steamid> <admin_steamid> <admin_name> <time in minutes/0 perm> <reason>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnWebGagCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (DatabaseProvider == null) return;

        if (!Helper.ValidateSteamId(command.GetArg(1), out var steamId) || steamId == null) return;
        if (!ulong.TryParse(command.GetArg(2), out var adminSteamId)) return;

        string adminName = command.GetArg(3);
        string webAdminName = $"[WEBGAG] {adminName}";
        var time = Math.Max(0, Helper.ParsePenaltyTime(command.GetArg(4)));
        var reason = command.ArgCount >= 6
            ? string.Join(" ", Enumerable.Range(5, command.ArgCount - 5).Select(command.GetArg)).Trim()
            : _localizer?["sa_unknown"] ?? "Unknown";
        reason = string.IsNullOrWhiteSpace(reason) ? _localizer?["sa_unknown"] ?? "Unknown" : reason;

        var adminInfo = PlayersInfo.TryGetValue(adminSteamId, out var info)
            ? info
            : new PlayerInfo(null, 0, new SteamID(adminSteamId), adminName, null, 0, 0, 0, 0, 0);

        Task.Run(async () =>
        {
            await Server.NextWorldUpdateAsync(() =>
            {
                var player = Helper.GetPlayerFromSteamid64(steamId.SteamId64);
                if (player != null && player.IsValid)
                {
                    Gag(null, player, time, reason, webAdminName, silent: true, overrideAdminInfo: adminInfo);
                    return;
                }

                Task.Run(async () =>
                {
                    int? penaltyId = await MuteManager.AddMuteBySteamid(steamId.SteamId64, adminInfo, reason, time, 3);
                    Helper.SendDiscordPenaltyMessage(null, steamId.SteamId64.ToString(), reason, time, PenaltyType.Gag, _localizer);

                    var activityArgs = time == 0
                        ? new object[] { webAdminName, steamId.SteamId64.ToString(), reason }
                        : new object[] { webAdminName, steamId.SteamId64.ToString(), reason, time };
                    var activityKey = time == 0 ? "sa_admin_gag_message_perm" : "sa_admin_gag_message_time";

                    await Server.NextWorldUpdateAsync(() =>
                    {
                        SimpleAdminApi?.OnPlayerPenaltiedAddedEvent(steamId, adminInfo, PenaltyType.Gag, reason, time, penaltyId);
                        Helper.ShowAdminActivity(activityKey, webAdminName, false, activityArgs);
                    });
                });
            });
        });
    }

    [CommandHelper(minArgs: 5, usage: "<steamid> <admin_steamid> <admin_name> <time in minutes/0 perm> <reason>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnWebSilenceCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (DatabaseProvider == null) return;

        if (!Helper.ValidateSteamId(command.GetArg(1), out var steamId) || steamId == null) return;
        if (!ulong.TryParse(command.GetArg(2), out var adminSteamId)) return;

        string adminName = command.GetArg(3);
        string webAdminName = $"[WEBSILENCE] {adminName}";
        var time = Math.Max(0, Helper.ParsePenaltyTime(command.GetArg(4)));
        var reason = command.ArgCount >= 6
            ? string.Join(" ", Enumerable.Range(5, command.ArgCount - 5).Select(command.GetArg)).Trim()
            : _localizer?["sa_unknown"] ?? "Unknown";
        reason = string.IsNullOrWhiteSpace(reason) ? _localizer?["sa_unknown"] ?? "Unknown" : reason;

        var adminInfo = PlayersInfo.TryGetValue(adminSteamId, out var info)
            ? info
            : new PlayerInfo(null, 0, new SteamID(adminSteamId), adminName, null, 0, 0, 0, 0, 0);

        Task.Run(async () =>
        {
            await Server.NextWorldUpdateAsync(() =>
            {
                var player = Helper.GetPlayerFromSteamid64(steamId.SteamId64);
                if (player != null && player.IsValid)
                {
                    Silence(null, player, time, reason, webAdminName, silent: true, overrideAdminInfo: adminInfo);
                    return;
                }

                Task.Run(async () =>
                {
                    int? penaltyId = await MuteManager.AddMuteBySteamid(steamId.SteamId64, adminInfo, reason, time, 2);
                    Helper.SendDiscordPenaltyMessage(null, steamId.SteamId64.ToString(), reason, time, PenaltyType.Silence, _localizer);

                    var activityArgs = time == 0
                        ? new object[] { webAdminName, steamId.SteamId64.ToString(), reason }
                        : new object[] { webAdminName, steamId.SteamId64.ToString(), reason, time };
                    var activityKey = time == 0 ? "sa_admin_silence_message_perm" : "sa_admin_silence_message_time";

                    await Server.NextWorldUpdateAsync(() =>
                    {
                        SimpleAdminApi?.OnPlayerPenaltiedAddedEvent(steamId, adminInfo, PenaltyType.Silence, reason, time, penaltyId);
                        Helper.ShowAdminActivity(activityKey, webAdminName, false, activityArgs);
                    });
                });
            });
        });
    }

    [CommandHelper(minArgs: 4, usage: "<steamid> <admin_steamid> <admin_name> <reason>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnWebKickCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (DatabaseProvider == null) return;

        if (!Helper.ValidateSteamId(command.GetArg(1), out var steamId) || steamId == null) return;
        if (!ulong.TryParse(command.GetArg(2), out var adminSteamId)) return;

        string adminName = command.GetArg(3);
        string webAdminName = $"[WEBKICK] {adminName}";
        var reason = command.ArgCount >= 5
            ? string.Join(" ", Enumerable.Range(4, command.ArgCount - 4).Select(command.GetArg)).Trim()
            : _localizer?["sa_unknown"] ?? "Unknown";
        reason = string.IsNullOrWhiteSpace(reason) ? _localizer?["sa_unknown"] ?? "Unknown" : reason;

        Task.Run(async () =>
        {
            await Server.NextWorldUpdateAsync(() =>
            {
                var player = Helper.GetPlayerFromSteamid64(steamId.SteamId64);
                if (player == null || !player.IsValid || !player.UserId.HasValue) return;

                Helper.KickPlayer(player.UserId.Value, NetworkDisconnectionReason.NETWORK_DISCONNECT_KICKBANADDED);
                Helper.DisplayCenterMessage(player, "sa_player_kick_message", webAdminName, new object[] { reason, webAdminName });
                Helper.ShowAdminActivity("sa_admin_kick_message", webAdminName, false, new object[] { webAdminName, player.PlayerName, reason });
                Helper.LogCommand(null, $"css_kick {player.SteamID} {reason}");
            });
        });
    }
}