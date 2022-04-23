using System.Runtime.Caching;

namespace App.Commands;

public class Casino : ModuleBase<SocketCommandContext>
{
    readonly Random rand = new();
    private readonly ObjectCache cache = MemoryCache.Default;
    private readonly ulong casinoPotChannelId = 967482594327425054;
    private readonly ulong casinoListMessageId = 967512001515487282;
    private readonly ulong userId = 109065356085047296;

    [Command("casino")]
    [Name("card casino")]
    [Summary("Rolls a card from the casino -- Currently only works in Ethans server")]
    public async Task CasinoCommand()
    {
        var user = Context.User;
        if(!IsCasinoOpen())
        {
            await ReplyAsync($"The casino is currently closed, please message <@{userId}> if you would like to play");
            return;
        }

        var casinoList = await GetCasinoList();
        if (casinoList == null) return;

        // Roll for a card
        var roll = rand.Next(0, casinoList.Length);
        var shuffledList = casinoList.OrderBy(a => Guid.NewGuid()).ToList(); //Create new, random guids for each element and just organize by that. Essentially a shuffle
        var rolledCard = shuffledList[roll];

        var message = await Context.Channel.SendMessageAsync($"{user.Username} has rolled {shuffledList[roll]}!");
        if(rolledCard.Contains("Jackpot!!") || rolledCard.Contains("jackpot!!"))
        {
            await message.AddReactionAsync(new Emoji("🎉"));
            await message.AddReactionAsync(new Emoji("🎊"));
        }
    }

    [Command("casinoremove")]
    [Name("removes card from casino pot")]
    [Summary("Removes card from casino pot -- Currently only works in Ethans server")]
    public async Task CasinoRemoveCommand([Remainder][Summary("Removes this card from the pot")] string cardId)
    {
        if (Context.User.Id != userId) return;

        var casinoList = await GetCasinoList();
        if (casinoList == null) return;

        casinoList = casinoList.Where(card => !card.Contains(cardId)).ToArray();
        var updatedList = string.IsNullOrWhiteSpace(string.Join('\n', casinoList)) ? "No cards left in the casino!" : string.Join('\n', casinoList);
        await Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(casinoListMessageId, m => m.Content = updatedList);
        await ReplyAsync($"{cardId} has been removed from the pot");
    }

    [Command("casinoadd")]
    [Name("adds card to casino pot")]
    [Summary("Adds card to casino pot -- Currently only works in Ethans server")]
    public async Task CasinoAddCommand([Remainder][Summary("Adds this card to the pot")] string fullCardText)
    {
        if (Context.User.Id != userId) return;

        var casinoList = await GetCasinoList();
        if (casinoList == null) return;

        var updatedList = casinoList.ToList();
        updatedList.Add(fullCardText);
        await Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(casinoListMessageId, m => m.Content = string.Join('\n', updatedList));
        await ReplyAsync($"{fullCardText} has been added to the pot");
    }

    [Command("casinopotfill")]
    [Name("sets casino pot")]
    [Summary("Fills casino pot with cards -- Currently only works in Ethans server")]
    public async Task CasinoPotCommand()
    {
        if (Context.User.Id != userId) return;

        var casinoPotChannel = Context.Guild.GetTextChannel(967482594327425054);
        if (casinoPotChannel == null)
        {
            await ReplyAsync($"casino-pot channel not found in this server.");
            return;
        };

        var cardList = new string[] { 
        
        };

        await casinoPotChannel.SendMessageAsync(string.Join('\n', cardList));
    }

    [Command("casinoshift")]
    [Name("opens and closes the casino")]
    [Summary("Opens and closes the casino -- Currently only works in Ethans server")]
    public async Task CasinoOpenCommand()
    {
        if (Context.User.Id != userId) return;

        cache.Set("casinoopen", !IsCasinoOpen(), DateTime.UtcNow.AddDays(1));
        await ReplyAsync($"The casino is now {(IsCasinoOpen() ? "open" : "closed")}!");
    }

    public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        
    }

    private bool IsCasinoOpen()
    {
        var casinoState = cache["casinoopen"];
        if (casinoState == null)
        {
            cache.Set("casinoopen", false, DateTime.UtcNow.AddDays(1));
            casinoState = cache["casinoopen"];
        }
        return (bool)casinoState;
    }

    private async Task<string[]?> GetCasinoList()
    {
        var casinoPotChannel = Context.Guild.GetTextChannel(casinoPotChannelId);
        if (casinoPotChannel == null)
        {
            await ReplyAsync($"casino-pot channel not found in this server.");
            return null;
        }

        // Get current pot
        var casinoListMessage = await casinoPotChannel.GetMessageAsync(casinoListMessageId);
        if (casinoListMessage == null)
        {
            await ReplyAsync($"Could not find message with card list.");
            return null;
        }
        return casinoListMessage.Content.Split('\n');
    }
}
