using Discord;
using Discord.Interactions;

namespace PKHeX.Discord.Axew;

/// <summary>
/// Base implementation providing
/// </summary>
[DefaultMemberPermissions(GuildPermission.SendMessages)]
[RequireBotPermission(GuildPermission.SendMessages)]
public abstract class SlashModuleBase : InteractionModuleBase<SocketInteractionContext>;