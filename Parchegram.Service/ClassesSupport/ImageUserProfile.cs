using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        // Constructor Manejamos el dato bool para identificar si se setea _createPath
        public ImageUserProfile(bool createPath)
        {
            if (createPath)
                _createPath = (formFile, size) => @$"C:\Media\Profile\{size}\{size}{Guid.NewGuid()}${formFile.FileName}";
        }

        // Constructor que mantiene el logger
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
                        string[] paths = new string[2] { _createPath(formFile, "S"), _createPath(formFile, "M") };
                        Image image = new Image();
                        byte[] imageProfile = Image.GetFile(formFile);
                        MagickImage[] images = ResizeImages(imageProfile);
                        CopyImagesToPath(images, paths, user.Id);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                }
            }
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
        private void CopyImagesToPath(MagickImage[] images, string[] paths, int idUser)
        {
            if (images.Length > 0)
            {
                using (var db = new ParchegramDBContext())
                {
                    try
                    {
                        UserImageProfile userImageProfile = db.UserImageProfile.Where(u => u.IdUser == idUser).FirstOrDefault();
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
                        userImageProfile.IdUser = idUser;

                        if (userImageProfile == null)
                            db.UserImageProfile.Add(userImageProfile);
                        else
                            db.UserImageProfile.Update(userImageProfile);

                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Consulta la imagen de perfil de un usuario segun su Id
        /// </summary>
        /// <param name="idUser">Id del usuario dueño de la imagen</param>
        /// <param name="size">Tamaño de la image S ó M</param>
        /// <returns>Imagen de perfil de usuario en byte[]</returns>
        public async Task<byte[]> GetImageUser(int idUser, char size)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    UserImageProfile userImageProfile = await db.UserImageProfile.Where(u => u.IdUser.Equals(idUser)).FirstOrDefaultAsync();
                    if (size.Equals('M'))
                        return await Image.GetFile(userImageProfile.PathImageS);
                    else
                        return await Image.GetFile(userImageProfile.PathImageM);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }
    }
}
