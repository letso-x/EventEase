using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public interface IAzureStorageService
    {
        Task<string?> UploadFileAsync(IFormFile file, string containerName);
        Task<bool> DeleteFileAsync(string blobName, string containerName);
        Task<Uri?> GetBlobUriAsync(string blobName, string containerName);
    }

    public class AzureStorageService : IAzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureStorageService> _logger;

        public AzureStorageService(BlobServiceClient blobServiceClient, ILogger<AzureStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage
        /// </summary>
        public async Task<string?> UploadFileAsync(IFormFile file, string containerName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Attempted to upload empty file");
                    return null;
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"File size {file.Length} exceeds maximum allowed size {maxFileSize}");
                    throw new InvalidOperationException("File size exceeds 10MB limit");
                }

                // Get container client
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure container exists
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Generate unique blob name
                string blobName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload file
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _logger.LogInformation($"File uploaded successfully: {blobName}");
                return blobName;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage
        /// </summary>
        public async Task<bool> DeleteFileAsync(string blobName, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobName))
                    return false;

                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                bool deleted = await blobClient.DeleteIfExistsAsync();
                if (deleted)
                {
                    _logger.LogInformation($"File deleted successfully: {blobName}");
                }
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the URI of a blob in Azure Blob Storage
        /// </summary>
        public async Task<Uri?> GetBlobUriAsync(string blobName, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobName))
                    return null;

                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Check if blob exists
                if (await blobClient.ExistsAsync())
                {
                    return blobClient.Uri;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting blob URI: {ex.Message}");
                return null;
            }
        }
    }
}
