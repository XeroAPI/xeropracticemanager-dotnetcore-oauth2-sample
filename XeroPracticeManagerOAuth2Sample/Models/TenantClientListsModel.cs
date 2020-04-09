using System;
using System.Collections.Generic;

namespace XeroPracticeManagerOAuth2Sample.Models
{
    public class TenantClientListsModel
    {
        public string LoggedInUser { get; set; }
        public List<(Guid tenantId, ClientApi.ClientListResponse clients)> TenantClients { get; set; }
    }
}
