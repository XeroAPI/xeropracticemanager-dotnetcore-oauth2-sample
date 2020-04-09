﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace XeroPracticeManagerOAuth2Sample.Example
{
    public class MemoryTokenStore
    {
        private readonly Dictionary<string, IXeroToken> _tokens;

        private readonly IXeroClient _xeroClient;

        public MemoryTokenStore(IXeroClient xeroClient)
        {
            _xeroClient = xeroClient;
            _tokens = new Dictionary<string, IXeroToken>();
        }

        public async Task<IXeroToken> GetAccessTokenAsync(string xeroUserId)
        {
            if (!_tokens.ContainsKey(xeroUserId))
            {
                return null;
            }

            var token = _tokens[xeroUserId];

            token = await _xeroClient.GetCurrentValidTokenAsync(token);

            SetToken(xeroUserId, token);

            return token;
        }

        public void SetToken(string xeroUserId, IXeroToken xeroToken)
        {
            _tokens[xeroUserId] = xeroToken;
        }
    }
}
