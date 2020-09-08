using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackgroundJob.Models;
using BackgroundJob.BackgroundJob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace BackgroundJob.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        public HomeController(ILogger<HomeController> logger, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ResizeImage(string srcDirectory, string destDirectory, int width)
        {
            if (string.IsNullOrWhiteSpace(srcDirectory)  || !Directory.Exists(srcDirectory))
            {
                TempData["Error"] = "Soure directory invalid!";
                return RedirectToAction(nameof(Index));
            }

            var filePaths = Directory.GetFiles(srcDirectory, "*.jpg");

            if(filePaths.Length == 0)
            {
                TempData["Error"] = "Soure directory is empty!";
                return RedirectToAction(nameof(Index));
            }

            string savePath = string.Empty;

            if (!string.IsNullOrWhiteSpace(destDirectory))
            {
                if(!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                savePath = destDirectory;
            } 
            else
            {
                savePath = Path.Combine(srcDirectory, "ImagesResized");

                if (!Directory.Exists(savePath))
                {
                    DirectoryInfo di = new DirectoryInfo(srcDirectory);
                    di.CreateSubdirectory("ImagesResized");
                }
            }

            _taskQueue.QueueBackgroundWorkItem(async cancellationToken =>
            {
                ResizeImages(filePaths, savePath, width);
            });
            TempData["Success"] = "Resize image completed!";
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void ResizeImages(string[] filePaths, string savePath, int width)
        {
            if (width == 0) width = 700;
            foreach (var file in filePaths)
            {
                using (Image image = Image.Load(file))
                {
                    int height = image.Height * width / image.Width;
                    image.Mutate(x => x.Resize(width, height));
                    image.Save(Path.Combine(savePath, Path.GetFileName(file)));
                }
            }
            
        }
    }
}
