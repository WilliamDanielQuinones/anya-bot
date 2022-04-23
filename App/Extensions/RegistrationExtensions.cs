namespace App.Extensions;

public static class RegistrationExtensions
{
    public static void RegisterClasses(this IServiceCollection collection)
    {
        collection.AddSingleton<IBot, Bot>();
    }

    public static void RegisterReactions(DiscordSocketClient _client)
    {
        //_client.ReactionAdded += Commands.Casino.ReactionAdded;
    }
}
