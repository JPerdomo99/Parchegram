using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Parchegram.Service.ClassesSupport
{
    public class MediaFile
    {
        private readonly ILogger<MediaFile> _logger;
        private const string FOLDERPATH = @"C:\Media\Post";
        private string fullPath;
        private IFormFile file;
        private string fullPathOriginal;
        private string nameGuid;

        public string FullPath { 
           get { return fullPath; }
        }

        public IFormFile File 
        {
            set { file = value; }
        }

        public MediaFile(IFormFile file)
        {
            this.file = file;
            nameGuid = Guid.NewGuid().ToString();
            fullPathOriginal = Path.Combine(FOLDERPATH, $"{nameGuid}{Path.GetExtension(this.file.FileName)}");
        }

        public MediaFile(ILogger<MediaFile> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene path del archivo que llega por post
        /// </summary>
        /// <param name="file">Archivo que llega desde un formulario</param>
        /// <returns>Ruta de donde se guardara el archivo</returns>
        public void GeneratePathFile()
        {
            try
            {
                if (file.ContentType.Contains("image"))
                    fullPath = Path.Combine(FOLDERPATH, $"{nameGuid}.png");
                else
                    fullPath = Path.Combine(FOLDERPATH, $"{nameGuid}.mp4");
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                fullPath = "";
            }
        }

        /// <summary>
        /// Copia el archivo en la ruta especificada
        /// </summary>
        /// <param name="file">Archivo que llega desde un formulario</param>
        private async Task SaveFile()
        {
            try
            {
                SaveFileOperation();
                if (!file.ContentType.Contains("mp4") && !file.ContentType.Contains("png"))
                {
                    if (file.ContentType.Contains("Video"))
                        await ConvertToMp4();
                    if (file.ContentType.Contains("Image"))
                        ConvertToPng(path, folderPath);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        private void SaveFileOperation()
        {
            using (var fs = System.IO.File.Create(fullPathOriginal))
            {
                file.CopyTo(fs);
            }
        }

        private async Task ConvertToMp4()
        {
            try
            {
                await FFmpeg.Conversions.FromSnippet.ToMp4(fullPathOriginal, fullPath);
                System.IO.File.Delete(fullPathOriginal);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        private void ConvertToPng(string path, string folderPath)
        {
            try
            {
                string testChangeFormat = Path.ChangeExtension(path, "png");
                MagickImage magickImage = new MagickImage(fullPath);
                magickImage.Write(folderPath, MagickFormat.Png);
                System.IO.File.Delete(path);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
            //await FFmpeg.Conversions.FromSnippet.ToWebM(path, Path.ChangeExtension(path, "png"));
            //File.Delete(path);
        }
    }
}
