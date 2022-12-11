using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Application;

public class DeleteCommand : ModuleBase<SocketCommandContext>
{
    [Command("Delete", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task Delete(Int32 quantity) => await this.Delete(quantity, Direction.Before, this.Context.Message.Id);

    [Command("DeleteAfter", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task DeleteAfter(Int32 quantity, UInt64 messageId) => await this.Delete(quantity, Direction.After, messageId);

    [Command("DeleteBefore", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task DeleteBefore(Int32 quantity, UInt64 messageId) => await this.Delete(quantity, Direction.Before, messageId);

    [Command("DeleteBulk", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task DeleteBulk(Int32 quantity) => await this.DeleteBulk(quantity, Direction.Before, this.Context.Message.Id);

    [Command("DeleteBulkAfter", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task DeleteBulkAfter(Int32 quantity, UInt64 messageId) => await this.DeleteBulk(quantity, Direction.After, messageId);

    [Command("DeleteBulkBefore", RunMode = RunMode.Async)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task DeleteBulkBefore(Int32 quantity, UInt64 messageId) => await this.DeleteBulk(quantity, Direction.Before, messageId);

    protected async Task Delete(Int32 quantity, Direction direction, UInt64 messageId)
    {
        var user = this.Context.User as SocketGuildUser;
        if (!user.Roles.Any(r => r.Name.ToUpperInvariant() == "ADMIN"))
        {
            return;
        }

        var options = new RequestOptions()
        {
            AuditLogReason =
                $"""
                "Command": "{nameof(this.Delete)}({quantity}, {direction}, {messageId})",
                "Timestamp": "{this.Context.Message.Timestamp}",
                "User": "{this.Context.Message.Author.Id}"
                """
        };

        var messages = await this.Context.Channel
            .GetMessagesAsync(
                fromMessageId: messageId,
                dir: direction,
                limit: quantity,
                options: options
            )
            .FlattenAsync();

        foreach (var message in messages)
        {
            await this.Context.Channel.DeleteMessageAsync(message);
        }

        await this.Context.Channel.DeleteMessageAsync(this.Context.Message);
    }

    protected async Task DeleteBulk(Int32 quantity, Direction direction, UInt64 messageId)
    {
        var user = this.Context.User as SocketGuildUser;
        if (!user.Roles.Any(r => r.Name.ToUpperInvariant() == "ADMIN"))
        {
            return;
        }

        var options = new RequestOptions()
        {
            AuditLogReason =
                $"""
                "Command": "{nameof(this.DeleteBulk)}({quantity}, {direction}, {messageId})",
                "Timestamp": "{this.Context.Message.Timestamp}",
                "User": "{this.Context.Message.Author.Id}"
                """
        };

        var messages = await this.Context.Channel
            .GetMessagesAsync(
                fromMessageId: messageId,
                dir: direction,
                limit: quantity,
                options: options
            )
            .FlattenAsync();

        var channel = (ITextChannel)this.Context.Channel;

        await channel.DeleteMessagesAsync(messages);

        await this.Context.Channel.DeleteMessageAsync(this.Context.Message);
    }
}
