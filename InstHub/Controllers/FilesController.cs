using InstHub.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InstHub.Controllers
{
    public class FilesController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FilesController> _logger;

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
    {
         {".txt", "text/plain"},
        {".pdf", "application/pdf"},
        {".doc", "application/msword"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".gif", "image/gif"},
        {".csv", "text/csv"},
        {".xml", "application/xml"},
        {".html", "text/html"},
        {".htm", "text/html"},
        {".zip", "application/zip"},
        {".rar", "application/x-rar-compressed"},
        {".7z", "application/x-7z-compressed"},
        {".tar", "application/x-tar"},
        {".gz", "application/gzip"},
        {".mp3", "audio/mpeg"},
        {".wav", "audio/wav"},
        {".mp4", "video/mp4"},
        {".mov", "video/quicktime"},
        {".avi", "video/x-msvideo"},
        {".wmv", "video/x-ms-wmv"},
        {".mkv", "video/x-matroska"},
        {".flv", "video/x-flv"},
        {".webm", "video/webm"},
        {".svg", "image/svg+xml"},
        {".ico", "image/x-icon"},
        {".bmp", "image/bmp"},
        {".rtf", "application/rtf"},
        {".psd", "image/vnd.adobe.photoshop"},
        {".ai", "application/postscript"},
        {".eps", "application/postscript"},
        {".ps", "application/postscript"},
        {".exe", "application/x-msdownload"},
        {".msi", "application/x-msdownload"},
        {".epub", "application/epub+zip"},
        {".mobi", "application/x-mobipocket-ebook"},
        {".azw", "application/vnd.amazon.ebook"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/vnd.oasis.opendocument.spreadsheet"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".apk", "application/vnd.android.package-archive"},
        {".jar", "application/java-archive"},
        {".java", "text/x-java-source"},
        {".class", "application/java-vm"},
        {".c", "text/x-c"},
        {".cpp", "text/x-c"},
        {".h", "text/x-c"},
        {".cs", "text/plain"},
        {".py", "text/x-python"},
        {".js", "application/javascript"},
        {".json", "application/json"},
        {".css", "text/css"},
        {".php", "application/x-httpd-php"},
        {".sql", "application/sql"},
        {".log", "text/plain"},
        {".md", "text/markdown"}
    };
        }

        public class FileViewModel
        {
            public string FileName { get; set; }
            public string Extension { get; set; }
        }

        public class UserFilesViewModel
        {
            public string UserName { get; set; }
            public List<FileViewModel> Files { get; set; }
        }

        public FilesController(UserManager<AppIdentityUser> userManager, IWebHostEnvironment env, ILogger<FilesController> logger)
        {
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAndCreateUserFolder(IFormFile file)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Unable to load user with ID '{UserId}'", _userManager.GetUserId(User));
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Формируем путь к папке пользователя
            var userFolderPath = Path.Combine(_env.WebRootPath, "usersfiles", user.Id);

            // Проверяем наличие папки, если её нет - создаем
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
                _logger.LogInformation("Created directory for user {UserId} at {Path}", user.Id, userFolderPath);
            }

            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine(userFolderPath, file.FileName);

                _logger.LogInformation("Uploading file {FileName} for user {UserId}", file.FileName, user.Id);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    TempData["StatusMessage"] = "Файл загружен";
                    _logger.LogInformation("File {FileName} uploaded successfully for user {UserId}", file.FileName, user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName} for user {UserId}", file.FileName, user.Id);
                    TempData["StatusMessage"] = "Error uploading file.";
                }
            }
            else
            {
                TempData["StatusMessage"] = "No file selected for upload";
                _logger.LogWarning("No file selected for upload by user {UserId}", user.Id);
            }

            return RedirectToAction("UserFiles");
        }

        public async Task<IActionResult> UserFiles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userFolderPath = Path.Combine(_env.WebRootPath, "usersfiles", user.Id);
            var files = Directory.Exists(userFolderPath) ? Directory.GetFiles(userFolderPath) : new string[0];
            var fileViewModels = new List<FileViewModel>();
            foreach (var file in files)
            {
                fileViewModels.Add(new FileViewModel
                {
                    FileName = Path.GetFileName(file),
                    Extension = Path.GetExtension(file).ToLower()
                });
            }

            var model = new UserFilesViewModel
            {
                UserName = user.UserName,
                Files = fileViewModels
            };

            return View(model);
        }

        public async Task<IActionResult> Download(string fileName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userFolderPath = Path.Combine(_env.WebRootPath, "usersfiles", user.Id);
            var filePath = Path.Combine(userFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(filePath), fileName);
        }

        public async Task<IActionResult> Delete(string fileName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userFolderPath = Path.Combine(_env.WebRootPath, "usersfiles", user.Id);
            var filePath = Path.Combine(userFolderPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["StatusMessage"] = "Файл успешно удален";
            }
            else
            {
                TempData["StatusMessage"] = "Файл не найден";
            }
            return RedirectToAction("UserFiles");
        }


        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }


    }
}
