using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace MCPUI.Components.Pages
{
    public partial class Chat
    {
        string prompt = "";
        string askUri = "http://localhost:5268/rag?query=";
        private bool IsLoading = false;
        List<ChatMessage> chatHistory = new();

        private List<UploadedFile> uploadedFiles = new();
        private string uploadsPath => Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
        protected void OnInitialized()
        {
            // Ensure uploads directory exists  
            Directory.CreateDirectory(uploadsPath);

            // Load existing files  
            LoadExistingFiles();
        }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await AskLLMAsync();
            }
        }
        private async Task AskLLMAsync()
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return;

            // Add user message to history  
            chatHistory.Add(new ChatMessage { Content = prompt, IsUser = true });
            var userMessage = prompt;

            try
            {
                IsLoading = true;

                // Fix: Ensure WithMessage is defined in ChatContext or its extensions  
                var chatResponse = await new HttpClient().GetAsync($"{askUri}{prompt}");

                // Add chat response to history  
                if(!chatResponse.IsSuccessStatusCode)
                {
                    chatHistory.Add(new ChatMessage { Content = "Error: Unable to get response from LLM.", IsUser = false });
                    IsLoading = false;
                    return;
                }
                chatHistory.Add(new ChatMessage { Content = await chatResponse.Content.ReadAsStringAsync(), IsUser = false });
            }
            catch (Exception ex)
            {
                chatHistory.Add(new ChatMessage { Content = $"Error: {ex.Message}", IsUser = false });
            }
            IsLoading = false;
            }
        public class ChatMessage
        {
            public string Content { get; set; } = "";
            public bool IsUser { get; set; }
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
        private async Task UploadFilesAsync(InputFileChangeEventArgs e)
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
}
