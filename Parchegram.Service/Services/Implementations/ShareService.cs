using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Share;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class ShareService : IShareService
    {
        private readonly ILogger _logger;

        public ShareService(ILogger<ShareService> logger)
        {
            _logger = logger;
        }

        public bool AddShare(ShareRequest shareRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Share oShare = new Share();
                    oShare.IdUser = shareRequest.IdUser;
                    oShare.IdPost = shareRequest.IdPost;
                    oShare.Date = DateTime.Now;
                    db.Share.Add(oShare);
                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        public bool DeleteShare(ShareRequest shareRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Share oShare = db.Share.Where(s => s.IdUser == shareRequest.IdUser 
                                    && s.IdPost == shareRequest.IdPost).FirstOrDefault();
                    db.Share.Remove(oShare);
                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }
    }
}
