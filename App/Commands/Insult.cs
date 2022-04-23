namespace App.Commands;

public class Insult : ModuleBase<SocketCommandContext>
{
    readonly Random rand = new();

    [Command("insult")]
    [Name("Insult command for daddydiane")]
    [Summary("Insults")]
    public async Task InsultCommand()
    {
        if (Context.User.Id != 709240065548746754) return;

        var insultList = new string[]
        {
            "Your aim is mid",
            "Washed up Plat",
            "I slept through Code Gayass",
            "I’m Hershey’s favorite"
        };

        var insult = rand.Next(0, insultList.Length);

        await ReplyAsync($"Hey <@{183792569640157185}>, <@{709240065548746754}> says: {insult}");
    }
}
