using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace App.S3;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3, ILogger<S3Service> logger)
    {
        _s3 = s3;
        _logger = logger;
    }

    public async Task<(Stream FileStream, string ContentType)> ReadFile(string bucketName, string fileName, string directory)
    {
        try
        {
            var fileTransferUtility = new TransferUtility(_s3);
            var bucketPath = !string.IsNullOrWhiteSpace(directory)
                ? bucketName + @"/" + directory
                : bucketName;
            var request = new GetObjectRequest()
            {
                BucketName = bucketPath,
                Key = fileName
            };

            try
            {
                var objectResponse = await fileTransferUtility.S3Client.GetObjectAsync(request);
                return (objectResponse.ResponseStream, objectResponse.Headers.ContentType);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return (null, null);
            }
            
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            if (amazonS3Exception.ErrorCode != null &&
            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
             amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
            {
                _logger.LogError("Please check the provided AWS Credentials.");
            }
            else
            {
                _logger.LogError("An error occurred with the message '{0}' when reading an object",
                    amazonS3Exception.Message);
            }

            return (null, null);
        }
    }

    public async Task<S3Object[]> GetObjects(string bucketName, string prefix)
    {
        try
        {
            // List all objects
            ListObjectsRequest listRequest = new ListObjectsRequest
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var listResponse = await _s3.ListObjectsAsync(listRequest);
            return listResponse.S3Objects.ToArray();
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            if (amazonS3Exception.ErrorCode != null &&
            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
             amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
            {
                _logger.LogError("Please check the provided AWS Credentials.");
            }
            else
            {
                _logger.LogError($"An error occurred with the message '{0}' when reading directory {prefix}",
                    amazonS3Exception.Message);
            }

            return null;
        }
    }
}
