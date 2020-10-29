using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Parchegram.Service.ClassesSupport
{
    public class FileStocker
    {
        #region Attributes
        private IFormFile file;
        private string fullPath;
        private string fullPathOriginal;
        private string nameFileGuid;
        private readonly ILogger<FileStocker> _logger;
        private const string FOLDER_PATH = @"C:\Media\Post";
        private const string FFMPEG_PATH = @"C:\ffmpeg\bin";
        #endregion

        #region Constructors
        public FileStocker(IFormFile file)
        {
            this.file = file;
            nameFileGuid = Guid.NewGuid().ToString();
            fullPathOriginal = Path.Combine(FOLDER_PATH, $"{nameFileGuid}{Path.GetExtension(file.FileName)}");
            GeneratePathFile();
        }

        public FileStocker(ILogger<FileStocker> logger)
        {
            _logger = logger;
        }
        #endregion

        #region Properties
        public string FullPath
        {
            get { return fullPath; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Guarda el archivo en el path indicado
        /// </summary>
        /// <param name="file">Archivo que llega por parametro</param>
        public async Task SaveFile()
        {
            try
            {
                if (file.ContentType.Contains("image"))
                    SaveImage();
                else
                    await SaveVideo();
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        /// <summary>
        /// Guardar imagen convertida a formato png
        /// </summary>
        private void SaveImage()
        {
            try
            {
                MagickImage magickImage = new MagickImage(file.OpenReadStream());
                magickImage.Resize(800, 0);
                magickImage.Write(fullPath, MagickFormat.Png);
                magickImage.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        /// <summary>
        /// Guarda video convertido a mp4
        /// </summary>
        /// <returns>Tarea que espera mientras se convierte el video</returns>
        private async Task SaveVideo()
        {
            try
            {
                using (var fs = File.Create(fullPathOriginal))
                {
                    file.CopyTo(fs);
                }
                FFmpeg.SetExecutablesPath(FFMPEG_PATH);
                IConversion conversion = await FFmpeg.Conversions.FromSnippet.ToMp4(fullPathOriginal, fullPath);
                await conversion.Start();
                File.Delete(fullPathOriginal);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        /// <summary>
        /// Genera el path final con el que se guardara el archivo
        /// </summary>
        /// <returns>Ruta de donde se guardara el archivo</returns>
        private void GeneratePathFile()
        {
            try
            {
                if (file.ContentType.Contains("image"))
                    fullPath = Path.Combine(FOLDER_PATH, $"{nameFileGuid}.png");
                else
                    fullPath = Path.Combine(FOLDER_PATH, $"{nameFileGuid}.mp4");
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                fullPath = "";
            }
        }
        #endregion
    }
}
