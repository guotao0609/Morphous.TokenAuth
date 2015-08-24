﻿using Microsoft.Owin.Security.OAuth;
using Orchard.Environment;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace HttpAuth.Providers {
    public class AuthProvider : OAuthAuthorizationServerProvider {
        private readonly Work<IMembershipService> _membershipServiceWork;

        public AuthProvider(Work<IMembershipService> membershipServiceWork) {
            _membershipServiceWork = membershipServiceWork;
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context) {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            
            var user = _membershipServiceWork.Value.ValidateUser(context.UserName, context.Password);
            if (user == null) {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
            } else {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                context.Validated(identity);
            }
            
            return Task.FromResult<object>(null);
        }
    }
}