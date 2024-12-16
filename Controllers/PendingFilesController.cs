using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualHead.Areas.Identity.Data;
using VisualHead.Models;
using VisualHead.Models.ViewModel;
using VisualHead.Areas.Identity.Data;
using VisualHead.Models.ViewModel;
using VisualHead.Models;
using Microsoft.AspNetCore.Authorization;


namespace VisualHead.Controllers
{
    [Authorize]
    public class PendingFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public PendingFilesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var pendingFiles = await _context.PendingFiles.Include(p => p.User).ToListAsync();
            return View(pendingFiles);
        }

        [HttpGet]
        public async Task<IActionResult> ApprovedFiles()
        {
            var approvedFiles = await _context.UploadedFiles.Include(f => f.User).ToListAsync();
            return View(approvedFiles);
        }

        [HttpGet]
        public IActionResult ViewApprovedPDF(int id)
        {
            var approvedFile = _context.UploadedFiles.FirstOrDefault(f => f.Id == id);
            if (approvedFile == null || string.IsNullOrEmpty(approvedFile.FilePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", approvedFile.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> EditApprovedFile(int id)
        {
            var approvedFile = await _context.UploadedFiles.FindAsync(id);
            if (approvedFile == null) return NotFound();

            var vm = new UploadedFileViewModel
            {
                Id = approvedFile.Id,
                Title = approvedFile.Title,
                Description = approvedFile.Description,
                FileName = approvedFile.FileName,
                FilePath = approvedFile.FilePath,
                UploadedAt = approvedFile.UploadedAt
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditApprovedFile(int id, UploadedFileViewModel vm, IFormFile newFile)
        {
            var approvedFile = await _context.UploadedFiles.FindAsync(id);
            if (approvedFile == null) return NotFound();


            approvedFile.Title = vm.Title;
            approvedFile.Description = vm.Description;

            if (newFile != null)
            {
                if (System.IO.File.Exists(approvedFile.FilePath))
                {
                    System.IO.File.Delete(approvedFile.FilePath);
                }

                var filename = DateTime.Now.ToString("ÿyyymmddhhmmss") + "_" + newFile.FileName;
                var path = $"{_configuration.GetSection("FileManagement:SystemFileUploads").Value}";
                var filepath = Path.Combine(path, filename);

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await newFile.CopyToAsync(stream);
                }

                approvedFile.FileName = filename;
                approvedFile.FilePath = filepath;
            }

            _context.UploadedFiles.Update(approvedFile);
            await _context.SaveChangesAsync();

            return RedirectToAction("ApprovedFiles");
        }


        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var pendingFile = await _context.PendingFiles.FindAsync(id);
            if (pendingFile == null) return NotFound();

            var approvedFile = new UploadedFile
            {
                UploadedAt = pendingFile.UploadedAt,
                FileName = pendingFile.FileName,
                Description = pendingFile.Description,
                FilePath = pendingFile.FilePath,
                Title = pendingFile.Title,
                UserId = pendingFile.UserId
            };

            _context.UploadedFiles.Add(approvedFile);
            _context.PendingFiles.Remove(pendingFile);

            var notification = new Notification
            {
                Nid = pendingFile.UserId, // Set Nid to the userId of the emailRequest
                message = "The file you submitted has been approved"
            };

            // Add the notification to the database
            _context.Notifications.Add(notification);

            // Remove the email request
            _context.PendingFiles.Remove(pendingFile);


            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var pendingFile = await _context.PendingFiles.FindAsync(id);
            if (pendingFile == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", pendingFile.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.PendingFiles.Remove(pendingFile);

            var notification = new Notification
            {
                Nid = pendingFile.UserId, // Set Nid to the userId of the emailRequest
                message = "The file you uploaded has been rejected"
            };

            // Add the notification to the database
            _context.Notifications.Add(notification);

            // Remove the email request
            _context.PendingFiles.Remove(pendingFile);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewPDF(int id)
        {
            var pendingFile = _context.PendingFiles.FirstOrDefault(f => f.Id == id);
            if (pendingFile == null || string.IsNullOrEmpty(pendingFile.FilePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", pendingFile.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApprovedFile(int id)
        {
            var approvedFile = await _context.UploadedFiles.FindAsync(id);
            if (approvedFile == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", approvedFile.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.UploadedFiles.Remove(approvedFile);
            await _context.SaveChangesAsync();

            return RedirectToAction("ApprovedFiles");
        }


    }
}
