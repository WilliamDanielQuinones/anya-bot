using Amazon.S3.Model;

namespace App.S3;

public interface IS3Service
{
    Task<(Stream FileStream, string ContentType)> ReadFile(string bucketName, string fileName, string directory);
    Task<S3Object[]> GetObjects(string bucketName, string prefix);
}
