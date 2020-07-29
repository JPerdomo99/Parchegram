using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.Service.ClassesSupport
{
    public class ImageUserProfile
    {
        private readonly ILogger _logger;
        // Create filePath on base to rootPath and fileName
        private Func<IFormFile, string, string> _createPath;

        // Constructor
        public ImageUserProfile(bool createPath)
        {
            if (createPath)
                _createPath = (formFile, size) => @$"C:\Media\Profile\{size}\{size}{Guid.NewGuid()}${formFile.FileName}";
        }

        public ImageUserProfile(ILogger<ImageUserProfile> logger)
        {
            _logger = logger;
        }

        public void SaveProfileImage(IFormFile formFile, string nameUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == nameUser).FirstOrDefault();
                    if (user != null)
                    {
                        UserImageProfile userImageProfile = (from userImage in db.UserImageProfile
                                                             where userImage.IdUser == user.Id
                                                             select userImage).FirstOrDefault();
                        if (userImageProfile == null)
                        {
                            string[] paths = new string[2] { _createPath(formFile, "S"), _createPath(formFile, "M") };
                            byte[] imageProfile = ConvertToByteArray(formFile);
                            MagickImage[] images = ResizeImages(imageProfile);
                            CopyImagesToPath(images, paths, user.NameUser);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                }
            }
        }

        /// <summary>
        /// Sobrecarga privada para obtener un archivo en byte[] directamente de un IFormFile
        /// </summary>
        /// <param name="formFile">Archivo que se llego a un controlador</param>
        /// <returns>Array de bytes que conforman el archivo</returns>
        private byte[] ConvertToByteArray(IFormFile formFile)
        {
            byte[] fileBytes = null;
            if (formFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    fileBytes = ms.ToArray();
                    string s = Convert.ToBase64String(fileBytes);
                    // act on the Base64 data
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Sobrecarga pulica para obtener un archivo en byte[] con ayuda de una ruta
        /// </summary>
        /// <param name="pathFile">Ruta del archivo</param>
        /// <returns>Array de bytes que conforman el archivo</returns>
        public async Task<byte[]> ConvertToByteArray(string pathFile)
        {
            byte[] result;
            using (FileStream file = File.Open(pathFile, FileMode.Open))
            {
                result = new byte[file.Length];
                await file.ReadAsync(result, 0, (int)file.Length);
            }

            return result;
        }

        /// <summary>
        /// Rendimeziona las imagines en 2 dimensiones diferentes
        /// </summary>
        /// <param name="imageProfile">Array de byte[] que conforman la imagen</param>
        /// <returns>Un arreglo con las dos imagenes</returns>
        private MagickImage[] ResizeImages(byte[] imageProfile)
        {
            MagickGeometry size = new MagickGeometry();
            size.IgnoreAspectRatio = true;

            size.Width = 50;
            size.Height = 50;

            MagickImage imageS = new MagickImage(imageProfile);
            imageS.Resize(size);

            size.Width = 100;
            size.Height = 100;
            MagickImage imageM = new MagickImage(imageProfile);
            imageM.Resize(size);

            return new MagickImage[2] { imageS, imageM };
        }
        
        /// <summary>
        /// Copia las imagenes a sus respectivas rutas y guarda la ruta en db
        /// </summary>
        /// <param name="images">Imagenes a guardar</param>
        /// <param name="paths">Rutas donde se guardaran las imagenes</param>
        /// <param name="nameUser">Nombre de usuario para guardar las rutas en db con relación a ese usuario</param>
        private void CopyImagesToPath(MagickImage[] images, string[] paths, string nameUser)
        {
            if (images.Length > 0)
            {
                using (var db = new ParchegramDBContext())
                {
                    try
                    {
                        User user = db.User.Where(u => u.NameUser == nameUser).FirstOrDefault();
                        UserImageProfile userImageProfile = new UserImageProfile();
                        for (int i = 0; i < images.Length; i++)
                        {
                            try
                            {
                                images[i].Write(paths[i]);
                                images[i].Dispose();
                                if (i == 0)
                                    userImageProfile.PathImageS = paths[i];
                                else
                                    userImageProfile.PathImageM = paths[i];
                            }
                            catch (Exception e)
                            {
                                _logger.LogInformation(e.Message);
                            }
                        }
                        userImageProfile.IdUser = user.Id;
                        db.UserImageProfile.Add(userImageProfile);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e.Message);
                    }
                }
            }
        }
    }
}
