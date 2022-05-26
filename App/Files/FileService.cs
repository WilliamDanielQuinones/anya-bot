using App.S3;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace App.Files;

public class FileService : IFileService
{
    private readonly IConfiguration _config;
    private readonly IS3Service _s3Service;

    private string S3Bucket { get; set; }

    public FileService(IConfiguration config, IS3Service s3Service)
    {
        _config = config;
        _s3Service = s3Service;

        S3Bucket = "anya-bot-card-casino";
    }

    public async Task<T[]> GetRemoteCsvFileContent<T>(string fileName, string directory, int? skipLines)
    {
        var response = await GetRemoteFileContent(fileName, directory);
        if (response == null) return null;
        return GetCsvFileContents<T>(new StreamReader(response), skipLines);
    }

    public async Task<(string FileName,DateTime? LastModified)> GetLatestExportFileMetaData(string directory)
    {
        var filesInBucket = await _s3Service.GetObjects(S3Bucket, directory).ConfigureAwait(false);
        return (filesInBucket.OrderByDescending(f => f.LastModified).FirstOrDefault()?.Key, filesInBucket.OrderByDescending(f => f.LastModified).FirstOrDefault()?.LastModified);
    }

    private async Task<Stream> GetRemoteFileContent(string fileName, string directory)
    {
        var response = await _s3Service.ReadFile(S3Bucket, fileName, directory).ConfigureAwait(false);
        return response.FileStream;
    }

    private T[] GetCsvFileContents<T>(StreamReader reader, int? skipLines)
    {
        var results = new List<T>();
        using (reader)
        {
            if (skipLines != null)
            {
                for (var i = 0; i < skipLines; i++)
                {
                    reader.ReadLine();
                }
            }
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            var csvReader = new CsvReader(reader, config);
            var fileContent = csvReader.GetRecords<T>();
            results = fileContent.ToList();
        }
        return results.ToArray();
    }
}
