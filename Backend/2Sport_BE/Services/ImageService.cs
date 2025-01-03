﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Google.Api.Gax.ResourceNames;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace _2Sport_BE.Services
{
    public interface IImageService
    {
        Task<CreateFolderResult> CreateFolder(string folderName);
        Task<List<string>> ListAllFoldersAsync();
        Task<ImageUploadResult> UploadImageToCloudinaryAsync(IFormFile file);
        Task<ImageUploadResult> UploadImageToCloudinaryAsync(string filePath);
        Task<ImageUploadResult> UploadImageToCloudinaryAsync(IFormFile file, string folder);
        Task<List<string>> ListImagesAsync(string folderName);
        Task<bool> DeleteAnImage(string fileName, string folderName);
    }
    public class ImageService : IImageService
    {
        public async Task<ImageUploadResult> UploadImageToCloudinaryAsync(IFormFile file)
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };
                    uploadResult = await cloudinary.UploadAsync(uploadParams);
                }
            }

            return uploadResult;
        }
        public async Task<ImageUploadResult> UploadImageToCloudinaryAsync(string filePath)
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
            var uploadResult = new ImageUploadResult();
            if (filePath is not null)
            {

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(filePath),
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                uploadResult = await cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;
        }

        public async Task<ImageUploadResult> UploadImageToCloudinaryAsync(IFormFile file, string folder)
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };
                    uploadResult = await cloudinary.UploadAsync(uploadParams);
                }
            }

            return uploadResult;
        }

        // List All Images in a Folder
        public async Task<List<string>> ListImagesAsync(string folderName)
        {
            var imageUrls = new List<string>();
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
            var listParams = new ListResourcesByPrefixParams()
            {
                Prefix = folderName + "/",  // Cloudinary uses a "folder_name/" prefix to identify folders
                Type = "upload",            // Specify the resource type (e.g., "upload" for images/files)
                MaxResults = 100            // Limit the number of results (default: 10, max: 500)
            };

            var resources = cloudinary.ListResources(listParams);

            if (resources != null)
            {
                foreach (var resource in resources.Resources)
                {
                    imageUrls.Add(resource.SecureUrl.ToString());
                }
            }

            return imageUrls;
        }

        public async Task<bool> DeleteAnImage(string fileName, string folderName)
        {
            try
            {
                // Step 1: Set up Cloudinary account credentials
                var imageUrls = new List<string>();
                DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
                Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));

                // Step 2: Specify the PublicId of the image to delete
                string publicId = $"{folderName}/{fileName}"; // Example PublicId ("folder_name/file_name")

                // Step 3: Delete the image
                var deletionParams = new DeletionParams(publicId)
                {
                    // Optional: Specify the resource type (default is "image")
                    ResourceType = ResourceType.Image
                };

                var deletionResult = cloudinary.Destroy(deletionParams);

                // Step 4: Output the result
                if (deletionResult.Result == "ok")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }

        public async Task<CreateFolderResult> CreateFolder(string folderName)
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));

            var createFolderResult = await cloudinary.CreateFolderAsync(folderName);

            return createFolderResult;
        }

        public async Task<List<string>> ListAllFoldersAsync()
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            var _cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
            var folders = new List<string>();
            var excludedFolders = new HashSet<string> { "samples", "blog_images", "2Sport" }; // Folders to exclude

            try
            {
                var credentials = _cloudinaryUrl.Split(new[] { "://", ":", "@" }, StringSplitOptions.None);
                var apiKey = credentials[1];
                var apiSecret = credentials[2];
                var cloudName = credentials[3];

                var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://api.cloudinary.com/v1_1/{cloudName}/folders");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                    var response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var folderResponse = JsonSerializer.Deserialize<CloudinaryFolderResponse>(jsonResponse, options);

                    if (folderResponse != null && folderResponse.Folders != null)
                    {
                        foreach (var folder in folderResponse.Folders)
                        {
                            if (!excludedFolders.Contains(folder.Name)) // Exclude specific folders
                            {
                                folders.Add(folder.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching folders: {ex.Message}");
            }

            return folders;
        }
    }

    public class CloudinaryFolderResponse
    {
        public List<Folder> Folders { get; set; }
        public string NextCursor { get; set; }
        public int TotalCount { get; set; }
    }

    public class Folder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ExternalId { get; set; }
    }
}
