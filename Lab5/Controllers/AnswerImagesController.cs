using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Lab5.Data;
using Lab5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab5.Controllers
{
    public class AnswerImagesController : Controller
    {
        private readonly AnswerImageDataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string earthContainerName = "earthimages";
        private readonly string computerContainerName = "computerimages";

        public AnswerImagesController(AnswerImageDataContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.AnswerImages.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile answerImage)
        {

            BlobContainerClient containerClient;
            // Create the container and return a container client object
            try
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(earthContainerName);
                //containerClient = await _blobServiceClient.CreateBlobContainerAsync(computerContainerName);
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException)
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(earthContainerName);
                //containerClient = await _blobServiceClient.CreateBlobContainerAsync(computerContainerName);
            }

            try
            {
                // create the blob to hold the data
                var blockBlob = containerClient.GetBlobClient(answerImage.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                using (var memoryStream = new MemoryStream())
                {
                    // copy the file data into memory
                    await answerImage.CopyToAsync(memoryStream);

                    // navigate back to the beginning of the memory stream
                    memoryStream.Position = 0;

                    // send the file to the cloud
                    await blockBlob.UploadAsync(memoryStream);
                    memoryStream.Close();
                }

                // add the photo to the database if it uploaded successfully
                var image = new AnswerImage();
                image.Url = blockBlob.Uri.AbsoluteUri;
                image.FileName = answerImage.FileName;

                _context.AnswerImages.Add(image);
                _context.SaveChanges();
            }
            catch (RequestFailedException)
            {
                View("Error");
            }

            return RedirectToAction("Index");
        }

        /* For multiple files, use this
        public async Task<IActionResult> Create(ICollection<IFormFile> files)
        {
            BlobContainerClient containerClient;
        
        // Create the container and return a container client object
            try
             {
                 containerClient = await _blobServiceClient.CreateBlobContainerAsync(earthContainerName);
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
             }
             catch (RequestFailedException e)
             {
                  containerClient = _blobServiceClient.GetBlobContainerClient(earthContainerName);
            }
            foreach (var file in files)
              {
                  try
                {
                     // create the blob to hold the data
                     var blockBlob = containerClient.GetBlobClient(file.FileName);
                     if (await blockBlob.ExistsAsync())
                    {
                        await blockBlob.DeleteAsync();
                    }
                     using (var memoryStream = new MemoryStream())
                     {
                         // copy the file data into memory
                         await file.CopyToAsync(memoryStream);
                         // navigate back to the beginning of the memory stream
                         memoryStream.Position = 0;
                         // send the file to the cloud
                         await blockBlob.UploadAsync(memoryStream);
                         memoryStream.Close();
                     }
                     // add the photo to the database if it uploaded successfully
                     var image = new Smiley();
                     image.Url = blockBlob.Uri.AbsoluteUri;
                     image.FileName = file.FileName;
                     _context.Smilies.Add(image);
                     _context.SaveChanges();
                 }
                 catch (RequestFailedException e)
                 {
                 }
             }
            return RedirectToAction("Index");
    }*/

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.AnswerImages
                .FirstOrDefaultAsync(m => m.AnswerImageId == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _context.AnswerImages.FindAsync(id);


            BlobContainerClient containerClient;
            // Get the container and return a container client object
            try
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(earthContainerName);
                //containerClient = _blobServiceClient.GetBlobContainerClient(computerContainerName);
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            try
            {
                // Get the blob that holds the data
                var blockBlob = containerClient.GetBlobClient(image.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                _context.AnswerImages.Remove(image);
                await _context.SaveChangesAsync();

            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            return RedirectToAction("Index");
        }
    }
}