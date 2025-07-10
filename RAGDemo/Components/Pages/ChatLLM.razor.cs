using MaIN.Core.Hub;
using MaIN.Core.Hub.Contexts;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
using static Microsoft.KernelMemory.DocumentUploadRequest;

namespace RAGDemo.Components.Pages
{
    public partial class ChatLLM
    {
        private List<UploadedFile> uploadedFiles = new();
        private string uploadsPath => Path.Combine(Environment.WebRootPath, "uploads");
        string messageToLLM = "";
        private bool IsLoading = false;

        List<ChatMessage> chatHistory = new();
        private ChatContext? chatInstance;

       
        protected override void OnInitialized()
        {
            // Ensure uploads directory exists
            Directory.CreateDirectory(uploadsPath);

            // Load existing files
            LoadExistingFiles();
        }

        private void LoadExistingFiles()
        {
            uploadedFiles.Clear();
            var files = Directory.GetFiles(uploadsPath);
            foreach (var filePath in files)
            {
                var originalName = Path.GetFileName(filePath).Split('_', 2).Last();
                uploadedFiles.Add(new UploadedFile
                {
                    Name = originalName,
                    Path = filePath
                });
            }
        }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SendMessage();
            }
        }
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(messageToLLM))
                return;

            // Add user message to history
            chatHistory.Add(new ChatMessage { Content = messageToLLM, IsUser = true });
            var userMessage = messageToLLM;
            messageToLLM = "";

            try
            {
                IsLoading = true;

                if (chatInstance == null)
                {
                    chatInstance = AIHub.Chat()
                        .WithModel("gemma3:4b");
                }

                var result = await chatInstance
                    .WithMessage(userMessage)
                    .WithFiles(uploadedFiles.Select(f => f.Path).ToList())
                    .CompleteAsync();

                // Add chat response to history
                chatHistory.Add(new ChatMessage { Content = result.Message.Content, IsUser = false });
            }
            catch (Exception ex)
            {
                chatHistory.Add(new ChatMessage { Content = $"Error: {ex.Message}", IsUser = false });
            }
            IsLoading = false;
        }
        private async Task LoadFiles(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                if (Path.GetExtension(file.Name).ToLower() == ".pdf")
                {
                    var fileName = Path.GetRandomFileName() + "_" + file.Name;
                    var filePath = Path.Combine(uploadsPath, fileName);

                    await using (var stream = File.Create(filePath))
                    {
                        await file.OpenReadStream().CopyToAsync(stream);
                    }

                    uploadedFiles.Add(new UploadedFile
                    {
                        Name = file.Name,
                        Path = filePath
                    });
                }
            }
        }
        private void RemoveFile(UploadedFile file)
        {
            try
            {
                if (File.Exists(file.Path))
                {
                    File.Delete(file.Path);
                }
                uploadedFiles.Remove(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }
        private void ClearFiles()
        {
            foreach (var file in uploadedFiles.ToList())
            {
                RemoveFile(file);
            }
        }
        public void Dispose()
        {
            ClearFiles();
        }
    }
    public class UploadedFile
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }
    public class ChatMessage
    {
        public string Content { get; set; } = "";
        public bool IsUser { get; set; }
    }
}
