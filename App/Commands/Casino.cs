using System.Runtime.Caching;

namespace App.Commands;

public class Casino : ModuleBase<SocketCommandContext>
{
    readonly Random rand = new();
    private readonly ObjectCache cache = MemoryCache.Default;
    private readonly ulong casinoPotChannelId = 961470303039541248;
    private readonly ulong[] casinoListMessageIds = new ulong[] { 967659677183782913, 967659678504988692, 967659679536787496 };
    private readonly ulong userId = 109065356085047296;

    [Command("casino")]
    [Name("casino help")]
    [Summary("Casino info")]
    public async Task CasinoHelpCommand()
    {
        var embed = new EmbedBuilder();
        embed.Color = Color.Red;

        embed.Title = "--------------------WELCOME TO THE CARD CASINO--------------------";
        embed.AddField("How to Play", "𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲 𝖳𝖮 𝖯𝖫𝖠𝖸 | Kmt Dann IN THE #card-casino-floor channel | USE COMMAND `d!casinoroll` | 𝖫𝖮𝖢𝖪-𝖨𝖭 𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲​​​​​ | CARD LIST IN #card-casino-info channel", true);

        await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("casinoroll")]
    [Name("casino card roll")]
    [Summary("Rolls a card from the casino -- Currently only works in Ethans server")]
    [Alias("cr")]
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

        await Context.Channel.SendMessageAsync($"{user.Username} has rolled {shuffledList[roll]}!");
        if(rolledCard.Contains("🎉"))
        {
            var message = await Context.Channel.SendMessageAsync($"Congratulations <@{user.Id}> on rolling the jackpot!");
            await message.AddReactionAsync(new Emoji("🎉"));
            await message.AddReactionAsync(new Emoji("🎊"));
        }
    }

    [Command("casinoremove")]
    [Name("removes card from casino pot")]
    [Summary("Removes card from casino pot -- Currently only works in Ethans server")]
    [Alias("cre")]
    public async Task CasinoRemoveCommand([Remainder][Summary("Removes this card from the pot")] string cardId)
    {
        if (Context.User.Id != userId) return;

        var casinoList = await GetCasinoList();
        if (casinoList == null) return;

        var updatedList = casinoList.Where(card => !card.Contains(cardId)).ToArray();
        await UpdateCasinoList(updatedList.ToArray());

        await ReplyAsync($"{cardId} has been removed from the pot");
    }

    [Command("casinoadd")]
    [Name("adds card to casino pot")]
    [Summary("Adds card to casino pot -- Currently only works in Ethans server")]
    [Alias("ca")]
    public async Task CasinoAddCommand([Remainder][Summary("Adds this card to the pot")] string fullCardText)
    {
        if (Context.User.Id != userId) return;

        var casinoList = await GetCasinoList();
        if (casinoList == null) return;

        var updatedList = casinoList.ToList();
        if(updatedList.Contains(fullCardText))
        {
            await ReplyAsync($"{fullCardText} is already in the pot");
            return;
        }
        updatedList.Add(fullCardText);
        await UpdateCasinoList(updatedList.ToArray());

        await ReplyAsync($"{fullCardText} has been added to the pot");
    }

    [Command("casinopotfill")]
    [Name("sets casino pot")]
    [Summary("Fills casino pot with cards -- Currently only works in Ethans server")]
    public async Task CasinoPotCommand()
    {
        if (Context.User.Id != userId) return;

        var casinoPotChannel = Context.Guild.GetTextChannel(casinoPotChannelId);
        if (casinoPotChannel == null)
        {
            await ReplyAsync($"casino-pot channel not found in this server.");
            return;
        };

        var cardList = new string[] {
            "94fm9t ◈2 Arcane Jinx","913m6x ◈3 Genshin Impact Ganyu","982bwh ◈3 Genshin Impact Ganyu","98f39r ◈2 Genshin Impact Xiao","9l4l37 ◈3 Genshin Impact Xiao","fmbk1k ◈3 Genshin Impact Zhongli","95wb1f ◈2 Jujutsu Kaisen Toge Inumaki","91dds7 ◈2 Horimiya Izumi Miyamura","98z9h3 ◈3 My Hero Academia Katsuki Bakugou","99577g ◈1 Genshin Impact Arataki Itto","93lf46 ◈3 My Hero Academia 2 Himiko Toga","9rmmh9 ◈2 Genshin Impact Venti","gs5nt1 ◈3 Genshin Impact Venti","96jp02 ◈3 The Case Study of Vanitas Vanitas","glsl2z ◈1 Fate/stay night Saber","9tlm5x ◈2 Genshin Impact Shenhe","9t366h ◈1 Haikyuu!! Tobio Kageyama","9rdxtl ◈2 Genshin Impact Klee","g7510z ◈2 Genshin Impact Albedo","9ztxq9 ◈3 Genshin Impact Albedo","99m8l1 ◈3 No Game No Life Shiro","fqz4m2 ◈3 Genshin Impact Sangonomiya Kokomi","f1t84b ◈4 Genshin Impact Yoimiya","f2g3ft ◈3 Genshin Impact Yoimiya","98z9b9 ◈3 JoJo's Bizarre Adventure: Golden Wind Giorno Giovanna","ggsc6k ◈2 JoJo's Bizarre Adventure: Golden Wind Giorno Giovanna","9mbxft ◈1 Mieruko-chan Miko Yotsuya","gc3xxg ◈2 Mieruko-chan Miko Yotsuya","glslbs ◈2 Demon Slayer: Kimetsu no Yaiba Kanao Tsuyuri","9jmnc5 ◈3 Your lie in April Kaori Miyazono","9h62d1 ◈3 Mob Psycho 100 Shigeo Kageyama","gtwj4v ◈1 Mob Psycho 100 Shigeo Kageyama","9bmqfw ◈3 Berserk Guts","9vl3jx ◈2 Hatsune Miku: Downloader Miku Hatsune","9qgdqz ◈3 NieR: Automata 2B","91zvlf ◈2 KonoSuba: God's blessing on this wonderful world! Aqua","9ztxb9 ◈3 Genshin Impact Paimon","9kwvxl ◈1 Genshin Impact Paimon","993f8m ◈4 Howl's Moving Castle Howl","9q8swb ◈3 Attack on Titan: The Final Season Pieck Finger","947bmr ◈3 Noragami Yato","gd5vf4 ◈2 Attack on Titan Sasha Braus","98b1zz ◈3 Genshin Impact Mona","9nb108 ◈3 Persona 5 the Animation Joker","97fbnx ◈1 Genshin Impact Yanfei","gprfs5 ◈2 One Piece Usopp","9z6bjr ◈3 To Your Eternity Fushi","9qgd02 ◈3 The Seven Deadly Sins Ban","gszq14 ◈2 Bakemonogatari Hitagi Senjogahara","9t766h ◈3 Genshin Impact Gorou","9vrlh6 ◈2 Genshin Impact Chongyun","9359z4 ◈3 The God of High School Jin Mori","9tlm98 ◈4 The God of High School Jin Mori","95djh1 ◈3 Haikyuu!! Tetsurou Kuroo","913vq9 ◈3 Genshin Impact Fischl","90lhdl ◈2 Doki Doki Literature Club! Natsuki","fbltk6 ◈3 Doki Doki Literature Club! Natsuki","f50j8l ◈4 Vinland Saga Thorfinn Thordarson","9d0hss ◈4 Genshin Impact Qiqi","g751h5 ◈2 Doki Doki Literature Club! Sayori","9b5jtk ◈1 Demon Slayer: Kimetsu no Yaiba Sabito","g4wl2k ◈3 Fullmetal Alchemist Roy Mustang","gplmdd ◈3 Attack on Titan Annie Leonhart","93dgv5 ◈3 Bakemonogatari Shinobu Oshino","fq3f68 ◈3 Genshin Impact Lumine","fwnjp0 ◈4 Tokyo Revengers Nahoya Kawata","9t766p ◈4 One-Punch Man Fubuki","9p27bg ◈3 No Game No Life Jibril","g610fh ◈3 Genshin Impact Amber","g1tf0f ◈2 Hololive EN Ceres Fauna","9wgcks ◈3 Komi-san wa, Komyushou desu. Najimi Osana","9rqdjp ◈3 Soul Eater Death the Kid","gfthg4 ◈1 Genshin Impact Noelle","9rqdjd ◈2 Genshin Impact Noelle","fw71b4 ◈3 Final Fantasy VII Cloud Strife","r3qfvz ◈1 Demon Slayer: Kimetsu no Yaiba Gyoumei Himejima","9ftr3v ◈3 Genshin Impact Aether","gl8w5g ◈3 Genshin Impact Sucrose","3pvz17 ◈1 JoJo's Bizarre Adventure: Golden Wind Bruno Bucciarati","gprfsb ◈3 My Hero Academia 4 Mirko","9qx04j ◈3 The Seven Deadly Sins: Signs of Holy War Escanor","913vml ◈3 Re:ZERO -Starting Life in Another World- Ferris","9rzwc1 ◈3 Kaguya-sama: Love Is War Yu Ishigami","982bpf ◈2 Hololive: Holo no Graffiti Kiryu Coco","fb18qn ◈4 Puella Magi Madoka Magica Homura Akemi","9bhwb2 ◈2 Jujutsu Kaisen Mahito","9grwf3 ◈3 Hunter x Hunter Leorio Paladiknight","9z6b55 ◈3 Soul Eater Maka Albarn","fzfn7m ◈4 JoJo's Bizarre Adventure: Stardust Crusaders Jean Pierre Polnareff","gh8vxg ◈2 JoJo's Bizarre Adventure: Diamond Is Unbreakable Rohan Kishibe","f40hnt ◈4 Highschool of the Dead Saeko Busujima","9fhs32 ◈3 Bakemonogatari Black Hanekawa","9fhsg8 ◈2 JoJo's Bizarre Adventure: Stone Ocean F.F.","g5qtfw ◈2 Final Fantasy VII Sephiroth","fw7159 ◈4 Saekano: How to Raise a Boring Girlfriend Megumi Katou","gk6nms ◈2 Ouran High School Host Club Haruhi Fujioka","g7q7gw ◈3 Tengen Toppa Gurren Lagann Yoko Littner","g610fk ◈2 Genshin Impact Yun Jin","g5qtgp ◈1 Sailor Moon Usagi Tsukino","9gfwg1 ◈3 Steins;Gate Mayuri Shiina","fbltmb ◈3 Genshin Impact Barbara","fgkj17 ◈3 Kuroko's Basketball Taiga Kagami","9kwvxf ◈3 My Hero Academia 3 Nejire Hado","fg0r3g ◈3 Pokémon Eevee","93mrs5 ◈3 My Hero Academia 3 Mirio Togata","pb6n06 ◈2 Nisekoi Kosaki Onodera","9f6277 ◈3 Noragami Yukine","9bhw55 ◈3 Persona 5 the Animation Goro Akechi","9dth9h ◈2 Pokémon: Black & White: Adventures in Unova N","gk6n06 ◈2 Kaguya-sama: Love Is War Kei Shirogane"
        };
        var chunkedList = Helpers.Split(cardList, 40).ToArray();

        foreach (var list in chunkedList)
        {
            await casinoPotChannel.SendMessageAsync(string.Join('\n', list));
        }
    }

    [Command("casinoshift")]
    [Name("opens and closes the casino")]
    [Summary("Opens and closes the casino -- Currently only works in Ethans server")]
    [Alias("cs")]
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
            await ReplyAsync($"card-casino-info channel not found in this server.");
            return null;
        }

        var cards = new List<string>();
        foreach (var messageId in casinoListMessageIds)
        {
            var message = await casinoPotChannel.GetMessageAsync(messageId);
            if (message == null)
            {
                await ReplyAsync($"Could not find message with card list.");
                break;
            }
            cards.AddRange(message.Content.Split('\n'));
        }

        return cards.ToArray();
    }

    private async Task UpdateCasinoList(string[] cardList)
    {
        var listChunks = Helpers.Split(cardList, 40).ToArray();
        for (var i = 0; i < casinoListMessageIds.Length; i++)
        {
            var messageId = casinoListMessageIds[i];
            var listChunk = listChunks[i];
            await Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(messageId, m => m.Content = string.Join('\n', listChunk));
        }
    }
}
