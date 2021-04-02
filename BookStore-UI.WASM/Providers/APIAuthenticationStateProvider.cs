using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookStore_UI.WASM.Providers
{
  public class APIAuthenticationStateProvider : AuthenticationStateProvider
  {

    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public APIAuthenticationStateProvider(ILocalStorageService localStorage,
      JwtSecurityTokenHandler tokenHandler)
    {
      _localStorage = localStorage;
      _tokenHandler = tokenHandler;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
      try
      {
        var savedToken = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(savedToken))
        {
          return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        //if token is expired remove it and change authentication state
        var tokenContent = _tokenHandler.ReadJwtToken(savedToken);
        //then we get the jwt security token
        var expiry = tokenContent.ValidTo;

        if(expiry < DateTime.Now)
        {
          await _localStorage.RemoveItemAsync("authToken");
          return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        //get claims from token, build authentication user object and
        var claims = ParseClaim(tokenContent);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "JWT"));//jwt
        //return authenticated person
        return new AuthenticationState(user);
      }
      catch (Exception)
      {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
      }
    }

    public async Task LogginIn()
    {
      var savedToken = await _localStorage.GetItemAsync<string>("authToken");
      var tokenContent = _tokenHandler.ReadJwtToken(savedToken);
      var claims = ParseClaim(tokenContent);
      var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "JWT"));
      var authState = Task.FromResult(new AuthenticationState(user));
      NotifyAuthenticationStateChanged(authState);
    }

    public void Logout()
    {
      var logout = new ClaimsPrincipal(new ClaimsIdentity());
      var authState = Task.FromResult(new AuthenticationState(logout));
      NotifyAuthenticationStateChanged(authState);
    }

    private IList<Claim> ParseClaim(JwtSecurityToken tokenContent)
    {
      var claims = tokenContent.Claims.ToList();
      //@context.User.Identity.Name
      //this is used here, parsing the claim for the same that has same value as the subject
      claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject));
      return claims;
    }
  }
}
