using App.Files;
using System.Runtime.Caching;

namespace App.Commands;

public class Casino : ModuleBase<SocketCommandContext>
{
    private readonly IFileService _fileService;

    public Casino(IFileService fileService)
    {
        _fileService = fileService;
    }
    
    readonly Random rand = new();
    private static readonly ObjectCache cache = MemoryCache.Default;
    private static readonly ulong casinoPotChannelId = 979520973009199114;
    private static readonly ulong[] casinoListMessageIds = new ulong[] { 983638909298831381, 983638910435459093, 983638912058679296, 983638913103065088 };
    private static readonly ulong DannUserId = 109065356085047296;
    private const int TotalPotSize = 110;
    private const int PotChunkSize = 30;
    private const string EmptyMessage = "---------";
    
    [Command("casino")]
    [Name("casino help")]
    [Summary("Casino info")]
    public async Task CasinoHelpCommand()
    {
        var embed = new EmbedBuilder();
        embed.Color = Color.Red;

        embed.Title = "--------------------WELCOME TO THE CARD CASINO--------------------";
        embed.AddField("How to Play", "𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲 𝖳𝖮 𝖯𝖫𝖠𝖸 | Kmt Dann IN THE #card-casino-floor channel | CALL ANYA-BOT BY USING THE COMMAND `d!casinoroll` OR `d!roll` | 𝖫𝖮𝖢𝖪-𝖨𝖭 𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲​​​​​ | CARD LIST IN #card-casino-pot channel", true);

        await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("casinoroll")]
    [Name("casino card roll")]
    [Summary("Rolls a card from the casino -- Currently only works in Ethans server")]
    [Alias("cr", "roll")]
    public async Task CasinoCommand([Remainder][Summary("Amount of times to roll: limit 10")] int rolls)
    {
        var user = Context.User;
        if(!IsCasinoOpen())
        {
            await ReplyAsync($"The casino is currently closed, please message <@{DannUserId}> if you would like to play");
            return;
        }

        if (rolls > 10)
        {
            await ReplyAsync($"You can only roll a maximum of 10 times at once");
            return;
        }

        for (int i = 0; i < rolls; i++)
        {
            await CasinoCommand();
        }
    }

    [Command("casinoroll")]
    [Name("casino card roll")]
    [Summary("Rolls a card from the casino -- Currently only works in Ethans server")]
    [Alias("cr", "roll")]
    public async Task CasinoCommand()
    {
        var user = Context.User;
        if (!IsCasinoOpen())
        {
            await ReplyAsync($"The casino is currently closed, please message <@{DannUserId}> if you would like to play");
            return;
        }

        var casinoList = await GetCasinoList(Context);
        if (casinoList == null) return;

        // Roll for a card
        var roll = rand.Next(0, casinoList.Length);
        var shuffledList = casinoList.OrderBy(a => Guid.NewGuid()).ToList(); //Create new, random guids for each element and just organize by that. Essentially a shuffle
        var rolledCard = shuffledList[roll];

        await Context.Channel.SendMessageAsync($"{user.Username} has rolled {shuffledList[roll]}!");
        if (rolledCard.Contains("🎉") || rolledCard.Contains(":tada:"))
        {
            var message = await Context.Channel.SendMessageAsync($"Congratulations <@{user.Id}> on rolling the jackpot!");
            await message.AddReactionAsync(new Emoji("🎉"));
            await message.AddReactionAsync(new Emoji("🎊"));
        }
        if (rolledCard.Contains("💰") || rolledCard.Contains(":moneybag:"))
        {
            var message = await Context.Channel.SendMessageAsync($"Oh? Rare drop! Congrats <@{user.Id}>!");
            await message.AddReactionAsync(new Emoji("💰"));
        }
    }

    [Command("casinoremove")]
    [Name("removes card from casino pot")]
    [Summary("Removes card from casino pot -- Currently only works in Ethans server")]
    [Alias("cre", "rem")]
    public async Task CasinoRemoveCommand([Remainder][Summary("Removes cards from the pot")] string cardIds)
    {
        if (Context.User.Id != DannUserId) return;
        
        var cards = cardIds.Split(',');

        var casinoList = await GetCasinoList(Context);
        
        if (casinoList == null) return;

        foreach (var card in cards)
        {
            casinoList = casinoList.Where(c => !c.Contains(card.Trim())).ToArray();
        }

        await UpdateCasinoList(casinoList);
        var word = cards.Length > 1 ? "have" : "has";
        await ReplyAsync($"{cardIds} {word} been removed from the pot!");
    }

    [Command("casinoadd")]
    [Name("adds card to casino pot")]
    [Summary("Adds card to casino pot -- Currently only works in Ethans server")]
    [Alias("ca", "add")]
    public async Task CasinoAddCommand([Remainder][Summary("Adds this card to the pot")] string cardIds)
    {
        if (Context.User.Id != DannUserId) return;

        var cards = cardIds.Split(',');

        var cardsAdded = new List<string>();

        var list = await GetCasinoList(Context);
        if (list == null) return;

        var currentCardList = list.ToList();

        foreach (var card in cards)
        {
            if (currentCardList.Contains(card))
            {
                await ReplyAsync($"{card} is already in the pot");
            }else
            {
                currentCardList.Add(card);
                cardsAdded.Add(card);
            }
        }

        if(cardsAdded.Count > 0)
        {
            await UpdateCasinoList(currentCardList.ToArray());
            var word = cards.Length > 1 ? "have" : "has";
            await ReplyAsync($"{string.Join(',', cardsAdded)} {word} been added to the pot");
        }
        
    }

    [Command("casinopotfill")]
    [Name("sets casino pot")]
    [Summary("Fills casino pot with cards -- Currently only works in Ethans server")]
    public async Task CasinoPotCommand()
    {
        if (Context.User.Id != DannUserId) return;
        
        var (FileName, LastModified) = await _fileService.GetLatestExportFileMetaData("");
        if (string.IsNullOrWhiteSpace(FileName))
        {
            await ReplyAsync($"No card spreadsheet found");
            return;
        }

        var cards = await _fileService.GetRemoteCsvFileContent<KarutaCard>(FileName, "", null);
        if (cards == null)
        {
            await ReplyAsync($"No cards found in file");
            return;
        }
        var casinoCards = cards.Where(c => c.Tag == "casino").OrderBy(c => rand.Next()).Take(TotalPotSize).Select(c => new CasinoCard(c)).OrderBy(c => c.Series).ToList();

        // Cut into chunks of 35 to account for discord's message limit
        var cardLists = casinoCards.Chunk(PotChunkSize).ToArray();
        var messages = new List<IUserMessage>();
        foreach (var cardList in cardLists)
        {
            if (cardList == null) continue;
            var cardString = string.Join("\n", cardList.Select(c => c.ToString()));
            var message = await ReplyAsync(cardString);
            messages.Add(message);
        }

        await ReplyAsync("Added all cards! IDs of messages are: " + string.Join(", ", messages.Select(m => m.Id)));
    }

    [Command("casinoshift")]
    [Name("opens and closes the casino")]
    [Summary("Opens and closes the casino -- Currently only works in Ethans server")]
    [Alias("cs")]
    public async Task CasinoOpenCommand()
    {
        if (Context.User.Id != DannUserId) return;

        cache.Set("casinoopen", !IsCasinoOpen(), DateTime.UtcNow.AddDays(1));
        await ReplyAsync($"The casino is now {(IsCasinoOpen() ? "open" : "closed")}!");
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

    private async Task<string[]?> GetCasinoList(SocketCommandContext context)
    {
        var casinoPotChannel = context.Guild.GetTextChannel(casinoPotChannelId);
        if (casinoPotChannel == null)
        {
            await ReplyAsync($"card-casino-info channel not found in this server.");
            return null;
        }

        var cards = new List<string>();
        var messageTasks = new List<Task<IMessage>>();
        foreach (var messageId in casinoListMessageIds)
        {
            messageTasks.Add(casinoPotChannel.GetMessageAsync(messageId));
        }

        var messages = await Task.WhenAll(messageTasks);
        foreach(var message in messages)
        {
            if (message.Content == EmptyMessage) continue;
            cards.AddRange(message.Content.Split('\n'));
        }
        return cards.ToArray();
    }

    private async Task UpdateCasinoList(string[] cardList)
    {
        var listChunks = cardList.Chunk(PotChunkSize).ToList();
        for (var i = 0; i < casinoListMessageIds.Length; i++)
        {
            var messageId = casinoListMessageIds[i];
            if(i < listChunks.Count)
            {
                var cardBatch = listChunks[i].ToList();
                cardBatch.RemoveAll(c => c == EmptyMessage || c == "\n");
                cardBatch = cardBatch.Select(c => c.ReplaceLineEndings("\n")).ToList();
                var newMessage = string.Join('\n', cardBatch);
                Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(messageId, m => m.Content = newMessage);
                Thread.Sleep(100);
            }else
            {
                Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(messageId, m => m.Content = EmptyMessage);
                Thread.Sleep(100);
            }
        }
    }
}
