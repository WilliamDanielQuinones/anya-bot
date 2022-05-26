namespace App.Files;

public interface IFileService
{
    Task<T[]> GetRemoteCsvFileContent<T>(string fileName, string directory, int? skipLines);

    Task<(string FileName, DateTime? LastModified)> GetLatestExportFileMetaData(string directory);
}
