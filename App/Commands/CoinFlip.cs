namespace App.Commands;

public  class CoinFlip : ModuleBase<SocketCommandContext>
{
    readonly Random rand = new();

    [Command("flip")]
    [Name("flip")]
    [Summary("Flips a coin")]
    [Alias("coin", "coinflip")]
    public Task Flip() => Context.Channel.SendMessageAsync(rand.Next(1, 3) == 1 ? "heads" : "tails");
}
