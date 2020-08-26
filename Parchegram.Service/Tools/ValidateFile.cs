using System;
using System.Linq;

namespace Parchegram.Service.Tools
{
    public class ValidateFile
    {
        private static string[] ImageExtensions = { "image/jpeg", "image/jpg", "image/png", "image/x-icon", "image/svg+xml", "image/gif" };
        private static string[] VideoExtensions = { "video/mp4", "video/x-ms-wmv", "video/3gpp", "video/avi" };

        /// <summary>
        /// Valida la extensión de la imagen
        /// </summary>
        /// <param name="extension">Extensión que se espera que este en el arreglo</param>
        /// <returns>bool</returns>
        public static bool ValidateExtensionImage(string extension)
        {
            try
            {
                string result = ImageExtensions.FirstOrDefault(e => e.Equals(extension));

                return (result != null) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida la extensión del archivo valido imagen o video
        /// </summary>
        /// <param name="extension"></param>
        /// <returns>bool</returns>
        public static bool ValidateExtensionFile(string extension)
        {
            try
            {
                string result = ImageExtensions.Concat(VideoExtensions).FirstOrDefault(e => e.Equals(extension));

                return (result != null) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida el tamaño del archivo
        /// </summary>
        /// <param name="size">Tamaño del archivo</param>
        /// <param name="limit">Limite aceptado</param>
        /// <returns>bool</returns>
        public static bool ValidateSizeFile(long size, long limit)
        {
            return (size <= limit) ? true : false;
        }

        /// <summary>
        /// Convierte bytes en megabytes para mandar como mensaje de respuesta
        /// </summary>
        /// <param name="size">Tamaño del archivo en bytes</param>
        /// <returns>Tamaño del archivo en megabytes con 2 decimales</returns>
        public static double ConvertToMegabytes(long size)
        {
            try
            {
                return Math.Round(Convert.ToDouble(size) / 1048576, 2);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
