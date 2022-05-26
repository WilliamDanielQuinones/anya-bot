using Amazon.S3;
using App.Files;
using App.S3;

namespace App.Extensions;

public static class RegistrationExtensions
{
    public static void RegisterClasses(this IServiceCollection collection)
    {
        collection.AddSingleton<IBot, Bot>();
        collection.AddSingleton<IS3Service, S3Service>();
        collection.AddSingleton<IFileService, FileService>();
        collection.AddAWSService<IAmazonS3>();
    }

    public static void RegisterReactions(DiscordSocketClient _client)
    {
        //_client.ReactionAdded += Commands.Casino.RemoveFromPotByReaction;
    }
}
