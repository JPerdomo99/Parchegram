using Microsoft.Extensions.Logging;
using Parchegram.Model.Request.Share;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IShareService
    {
        public bool AddShare(ShareRequest shareRequest);
        public bool DeleteShare(ShareRequest shareRequest);
    }
}
