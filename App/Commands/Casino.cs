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
        embed.AddField("How to Play", "𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲 𝖳𝖮 𝖯𝖫𝖠𝖸 | Kmt Dann IN THE #card-casino-floor channel | CALL ANYA-BOT BY USING THE COMMAND `d!casinoroll` OR `d!roll` | 𝖫𝖮𝖢𝖪-𝖨𝖭 𝟥 𝖳𝖨𝖢𝖪𝖤𝖳𝖲​​​​​ | CARD LIST IN #card-casino-info channel", true);

        await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("casinoroll")]
    [Name("casino card roll")]
    [Summary("Rolls a card from the casino -- Currently only works in Ethans server")]
    [Alias("cr", "roll")]
    public async Task CasinoCommand()
    {
        var user = Context.User;
        if(!IsCasinoOpen())
        {
            await ReplyAsync($"The casino is currently closed, please message <@{userId}> if you would like to play");
            return;
        }

        var casinoList = await GetCasinoList(Context);
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
    [Alias("cre", "rem")]
    public async Task CasinoRemoveCommand([Remainder][Summary("Removes cards from the pot")] string cardIds)
    {
        if (Context.User.Id != userId) return;

        var cards = cardIds.Split(',');

        var casinoList = await GetCasinoList(Context);
        if (casinoList == null) return;

        foreach (var card in cards)
        {
            casinoList = casinoList.Where(c => !c.Contains(card.Trim())).ToArray();
            await UpdateCasinoList(casinoList.ToArray());
        }
        await ReplyAsync($"{cardIds} have been removed from the pot");
    }

    [Command("casinoadd")]
    [Name("adds card to casino pot")]
    [Summary("Adds card to casino pot -- Currently only works in Ethans server")]
    [Alias("ca", "add")]
    public async Task CasinoAddCommand([Remainder][Summary("Adds this card to the pot")] string cardIds)
    {
        if (Context.User.Id != userId) return;

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
                await UpdateCasinoList(currentCardList.ToArray());
                cardsAdded.Add(card);
            }
            
        }
        await ReplyAsync($"{string.Join(',', cardsAdded)} have been added to the pot");
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
            await ReplyAsync($"card-casino-info channel not found in this server.");
            return;
        };

        var cardList = new string[] {
            "fx0ls4 Attack on Titan · Annie Leonhart",
            "gplmdd Attack on Titan · Annie Leonhart",
            "gd5vf4 Attack on Titan · Sasha Braus",
            "9q8swb Attack on Titan: The Final Season · Pieck Finger",
            "gszq14 Bakemonogatari · Hitagi Senjogahara",
            "9bmqfw Berserk · Guts",
            "gx1mfs Death Note · Light Yagami",
            "9p279k Death Note · Misa Amane",
            "9p0hsj Demon Slayer: Kimetsu no Yaiba · Genya Shinazugawa",
            "r3qfvz Demon Slayer: Kimetsu no Yaiba · Gyoumei Himejima",
            "9b5jtk Demon Slayer: Kimetsu no Yaiba · Sabito",
            "fbltk6 Doki Doki Literature Club! · Natsuki",
            "f5xxbz ERASED · Satoru Fujinuma",
            "fnsz80 Fairy Tail · Zeref",
            "fw71b4 Final Fantasy VII · Cloud Strife",
            "g5qtfw Final Fantasy VII · Sephiroth",
            "g7510z Genshin Impact · Albedo",
            "9ztxq9 Genshin Impact · Albedo",
            "g610fh Genshin Impact · Amber",
            "99577g Genshin Impact · Arataki Itto",
            "fbltmb Genshin Impact · Barbara",
            "9vrlh6 Genshin Impact · Chongyun",
            "9957s4 Genshin Impact · Dodoco",
            "913m6x Genshin Impact · Ganyu",
            "g6qplt Genshin Impact · Kairagi: Dancing Thunder",
            "9rdxtl Genshin Impact · Klee",
            "fq3f68 Genshin Impact · Lumine",
            "98b1zz Genshin Impact · Mona",
            "gfthg4 Genshin Impact · Noelle",
            "9ztxb9 Genshin Impact · Paimon",
            "9kwvxl Genshin Impact · Paimon",
            "fb18vf Genshin Impact · Qiu'ge",
            "fqz4m2 Genshin Impact · Sangonomiya Kokomi",
            "gl8w5g Genshin Impact · Sucrose",
            "9b5j8k Genshin Impact · Timmie",
            "9rmmh9 Genshin Impact · Venti",
            "gs5nt1 Genshin Impact · Venti",
            "98f39r Genshin Impact · Xiao",
            "9l4l37 Genshin Impact · Xiao",
            "96x863 Genshin Impact · Xinyan",
            "f1t84b Genshin Impact · Yoimiya",
            "g610fk Genshin Impact · Yun Jin",
            "fmbk1k Genshin Impact · Zhongli",
            "9t366h Haikyuu!! · Tobio Kageyama",
            "9vl3jx Hatsune Miku: Downloader · Miku Hatsune",
            "f40hnt Highschool of the Dead · Saeko Busujima",
            "91dds7 Horimiya · Izumi Miyamura",
            "993f8m Howl's Moving Castle · Howl",
            "93qctb Hunter x Hunter · Kite",
            "98z9b9 JoJo's Bizarre Adventure: Golden Wind · Giorno Giovanna",
            "ggsc6k JoJo's Bizarre Adventure: Golden Wind · Giorno Giovanna",
            "fzfn7m JoJo's Bizarre Adventure: Stardust Crusaders · Jean Pierre Polnareff",
            "947bhm JoJo's Bizarre Adventure: Steel Ball Run · Gyro Zeppeli",
            "9fhsg8 JoJo's Bizarre Adventure: Stone Ocean · F.F.",
            "9bhwb2 Jujutsu Kaisen · Mahito",
            "95wb1f Jujutsu Kaisen · Toge Inumaki",
            "gz5vtb Jujutsu Kaisen · Utahime Iori",
            "gk6n06 Kaguya-sama: Love Is War · Kei Shirogane",
            "9rzwc1 Kaguya-sama: Love Is War · Yu Ishigami",
            "9wgcks Komi-san wa, Komyushou desu. · Najimi Osana",
            "9mbxft Mieruko-chan · Miko Yotsuya",
            "gc3xxg Mieruko-chan · Miko Yotsuya",
            "g751qz Miss Kobayashi's Dragon Maid · Elma",
            "pwwqg7 Miss Kobayashi's Dragon Maid · Fafnir",
            "gtwjp7 Miss Kobayashi's Dragon Maid · Kanna Kamui",
            "91b2m6 Miss Kobayashi's Dragon Maid · Kobayashi",
            "91kkvw Miss Kobayashi's Dragon Maid · Lucoa",
            "gfvxxb Miss Kobayashi's Dragon Maid · Riko Saikawa",
            "g496db Miss Kobayashi's Dragon Maid · Shouta Magatsuchi",
            "9ftrgb Miss Kobayashi's Dragon Maid S · Ilulu",
            "9h62d1 Mob Psycho 100 · Shigeo Kageyama",
            "gtwj4v Mob Psycho 100 · Shigeo Kageyama",
            "93lf46 My Hero Academia 2 · Himiko Toga",
            "9kwvxf My Hero Academia 3 · Nejire Hado",
            "gprfsb My Hero Academia 4 · Mirko",
            "9qgdqz NieR: Automata · 2B",
            "9p27bg No Game No Life · Jibril",
            "99m8l1 No Game No Life · Shiro",
            "9f6277 Noragami · Yukine",
            "gprfs5 One Piece · Usopp",
            "9t766p One-Punch Man · Fubuki",
            "9tlm5v Oshi no Ko · Ruby Hoshino",
            "fppvxj Osomatsu-san · Obama",
            "9grw3w Persona 5 Royal · Kasumi Yoshizawa",
            "g7512r Persona 5 Royal · Violet",
            "grtl2j Persona 5 the Animation · Ann Takamaki",
            "g751hj Persona 5 the Animation · Haru Okumura",
            "95nkqb Persona 5 the Animation · Navi",
            "gk6nq5 Persona 5 the Animation · Noir",
            "g610pv Persona 5 the Animation · Panther",
            "9dth9h Pokémon: Black & White: Adventures in Unova · N",
            "fb18qn Puella Magi Madoka Magica · Homura Akemi",
            "fmgrp6 Ranma ½ · Genma Saotome",
            "fmgrr7 Ranma ½ · Ranma Saotome",
            "913vml Re:ZERO -Starting Life in Another World- · Ferris",
            "fw7159 Saekano: How to Raise a Boring Girlfriend · Megumi Katou",
            "9rzkq1 Soul Eater · Black Star",
            "9z6b55 Soul Eater · Maka Albarn",
            "9pb4c5 Steins;Gate · Kurisu Makise",
            "9gfwg1 Steins;Gate · Mayuri Shiina",
            "g7q7gw Tengen Toppa Gurren Lagann · Yoko Littner",
            "9tlm98 The God of High School · Jin Mori",
            "9qgd02 The Seven Deadly Sins · Ban",
            "9qx04j The Seven Deadly Sins: Signs of Holy War · Escanor",
            "9z6bjr To Your Eternity · Fushi",
            "fwnjp0 Tokyo Revengers · Nahoya Kawata",
            "f50j8l Vinland Saga · Thorfinn Thordarson",
            "9jmnc5 Your lie in April · Kaori Miyazono"};
        await UpdateCasinoList(cardList);
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

    private async Task<string[]?> GetCasinoList(SocketCommandContext context)
    {
        var casinoPotChannel = context.Guild.GetTextChannel(casinoPotChannelId);
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
        var emptyMessage = "---------";
        for (var i = 0; i < casinoListMessageIds.Length; i++)
        {
            var messageId = casinoListMessageIds[i];
            if(i < listChunks.Length)
            {
                var cardBatch = listChunks[i];
                cardBatch.RemoveAll(c => c == emptyMessage);
                var newMessage = string.Join('\n', cardBatch);
                await Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(messageId, m => m.Content = newMessage);
                Thread.Sleep(1000);
            }else
            {
                await Context.Guild.GetTextChannel(casinoPotChannelId).ModifyMessageAsync(messageId, m => m.Content = emptyMessage);
                Thread.Sleep(1000);
            }
        }
    }
}
