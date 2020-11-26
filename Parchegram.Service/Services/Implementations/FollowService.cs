using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Implementations
{
    public class FollowService : IFollowService
    {
        private readonly ILogger _logger;

        public FollowService(ILogger<FollowService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Agrega el follow segun los parametros correspondientes
        /// </summary>
        /// <param name="nameUserFollower">Nombre del usuario que va ser follower</param>
        /// <param name="idUserFollowing">Id del usuario que va a ser seguido</param>
        /// <returns>Objeto response que indica los resultados de la operación</returns>
        public async Task<Response> Add(string nameUserFollower, int idUserFollowing)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User userFollower = await db.User.Where(u => u.NameUser.Equals(nameUserFollower)).FirstOrDefaultAsync();
                    if (userFollower == null)
                        return response.GetResponse("El seguidor no existe", 0, false);
                    User userFollowing = await db.User.Where(u => u.Id.Equals(idUserFollowing)).FirstOrDefaultAsync();
                    if (userFollowing == null)
                        return response.GetResponse("El que va a ser seguido no existe", 0, false);
                    if (await GetFollow(userFollower.Id, userFollowing.Id))
                        return response.GetResponse("Intenta agregar un follow que ya existe", 0, false);

                    Follow follow = new Follow();
                    follow.IdUserFollower = userFollower.Id;
                    follow.IdUserFollowing = userFollowing.Id;
                    
                    db.Follow.Add(follow);
                    if (db.SaveChanges() == 1)
                        return response.GetResponse("Se ha agregado el follow correctamente", 1, true);
                    return response.GetResponse("No se agragego el follow :(", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado al momento de crear el follow {e.Message}", 0, true);
            }
        }

        /// <summary>
        /// Eliminar el follow segun los parametros correspondientes
        /// </summary>
        /// <param name="nameUserFollower">Nombre del usuario que es</param>
        /// <param name="idUserFollowing">Id del usuario que es seguido</param>
        /// <returns>Objeto response que indica los resultados de la operación</returns>
        public async Task<Response> Delete(string nameUserFollower, int idUserFollowing)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User userFollower = await db.User.Where(u => u.NameUser.Equals(nameUserFollower)).FirstOrDefaultAsync();
                    if (userFollower == null)
                        return response.GetResponse("El seguidor no existe", 0, false);
                    User userFollowing = await db.User.Where(u => u.Id.Equals(idUserFollowing)).FirstOrDefaultAsync();
                    if (userFollowing == null)
                        return response.GetResponse("El que va a ser seguido no existe", 0, false);
                    if (await GetFollow(userFollower.Id, userFollowing.Id) == false)
                        return response.GetResponse("El follow que desea borrar no existe", 0, false);

                    Follow follow = await db.Follow.Where(f => f.IdUserFollower.Equals(userFollower.Id) && f.IdUserFollowing.Equals(userFollowing.Id)).FirstOrDefaultAsync();

                    db.Follow.Remove(follow);
                    if (db.SaveChanges() == 1)
                        return response.GetResponse("Se ha eliminado el follow correctamente", 1, true);
                    return response.GetResponse("No se elimino el follow :(", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado al momento de eliminar el follow {e.Message}", 0, true);
            }
        }

        private async Task<bool> GetFollow(int idUserFollower, int idUserFollowing)
        {
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    return await db.Follow.Where(f => f.IdUserFollower.Equals(idUserFollower) && f.IdUserFollowing.Equals(idUserFollowing)).AnyAsync();
                }
            } catch(Exception e)
            {
                _logger.LogInformation(e.Message);
                return false;
            }
        }
    }
}
