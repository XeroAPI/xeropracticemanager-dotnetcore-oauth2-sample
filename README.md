# Xero Practice Manager OAuth2 Sample

This is an example dotnet core MVC application making use of Xero sign in, and Xero Practice Manager API access using OAuth2.0.  
The use case of our app is to sign users in user Xero Sign In and retrieve and display list of clients for the user's connected organisations.   

**Pre-requisite**:
- This sample is a .NET Core 2.2 application, so you'll need to install [.NET Core SDK 2.2 or above](https://dotnet.microsoft.com/download).
- You may want to read through Xero's [OAuth2.0 documentation](https://developer.xero.com/documentation/oauth2/overview) to familiarise yourself with the OAuth2.0 flow.

## Getting started with this sample.

1. [Create an OAuth2.0 app](https://developer.xero.com/documentation/oauth2/sign-in#createapp).
   - **Note**: When creating your app, be sure to add the correct redirect uri. You'll need one for `http://localhost:5000/signin-oidc` as the sample will run on port 5000 and bootstrap callback paths for `/signin-oidc`.  
   ![Redirect Uri](Docs/Images/redirecturis.png)
2. [Contact our support team](https://developer.xero.com/contact-xero-developer-platform-support/) to start your [security self assessment](https://devblog.xero.com/were-raising-our-platform-s-global-security-standards-9a058311943d) and get the practice manager scope added to your app.
3. Add the "Authorise 3rd Party Full Access" privilege to your user role on the [staff settings page](https://my.workflowmax.com/admin/resourcelist.aspx) of your Xero Practice Manager account.
4. Update the [appsettings.json](XeroPracticeManagerOAuth2Sample/appsettings.json) file to include your ClientId and ClientSecret.
5. Run the sample from the command line in the [XeroPracticeManagerOAuth2Samples-DotNetCore\XeroPracticeManagerOAuth2Sample\XeroPracticeManageraxOAuth2Sample](XeroPracticeManagerOAuth2Sample) directory using the command `dotnet run`, or using your favorite IDE.

## Ok, cool! How does it work though?

### Setting up authentication with Xero as your auth provider.

The bulk of the authentication settings are set up in the [Startup.cs](XeroPracticeManagerOAuth2Sample/Startup.cs) file.

Looking at the *ConfigureServices* method of the `Startup` class, you'll find a section of code for adding how this sample should be secured.

1. Before we get to that though, we've got to register some classes and services as dependencies into dependency injection.  

```
services.AddHttpClient();

services.TryAddSingleton(new XeroConfiguration
{
    ClientId = Configuration["Xero:ClientId"],
    ClientSecret = Configuration["Xero:ClientSecret"]
});

services.TryAddSingleton<IXeroClient, XeroClient>();
services.TryAddSingleton<MemoryTokenStore>();
```

`services.AddHttpClient` registers an instantiation of `IHttpClientFactory`, which along with the `XeroConfiguration` singleton, are constructor parameters for the `XeroClient` instantiation of `IXeroClient`.  
The `XeroClient` class interfaces with the Xero API via OAuth2.0 and OIDC. It's provided via the Nuget package: [Xero.NetStandard.OAuth2Client](https://www.nuget.org/packages/Xero.NetStandard.OAuth2Client).  
The `MemoryTokenStore` singleton is just a very simple token store using an in memory dictionary as token storage. It will be used to store access/refresh tokens against Xero UserIds to be used when retrieving data from Xero's API. The implementation of `MemoryTokenStore` can be found [here](XeroPracticeManagerOAuth2Sample/Example/MemoryTokenStore.cs).

2. Now, we'll look at how authentication is implemented for this sample.  
Firstly we're stating that we want to use cookies as our default authentication scheme, and naming the challenge scheme (*"XeroSignIn"*) that we'd like to use as our default challenge scheme. You'll see this challenge scheme (along with the *XeroSignUp* challenge scheme) implemented in the next section of code, along with an explanation of the differences between the two. We've chosen the *XeroSignIn* challenge scheme as the default because our users are more likely to want to sign into our app rather than sign up new connections when revisiting our app.

```
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "XeroSignIn";
})
.AddCookie(options =>
{
    options.Cookie.Name = "XeroIdentity";
})
```

3. Next we outline the implementation of our challenge scheme.  
This section is where your OAuth2.0 client details, and requested scopes are being set, along with some other required OpenID Connect parameters. It's also where we're stating that we'd like to use "https://identity.xero.com" as our auth provider.  
When granted, these scopes will allow your app to access subsets of the organisation's data on behalf of the logged in user. The `offline_access` scope allows your app to receive a refresh token on successful login. More details on these scopes can be found in Xero's [scopes documentation](https://developer.xero.com/documentation/oauth2/scopes)

```
.AddOpenIdConnect("XeroSignIn", options =>
{
	options.Authority = "https://identity.xero.com";

	options.ClientId = Configuration["Xero:ClientId"];
	options.ClientSecret = Configuration["Xero:ClientSecret"];

	options.ResponseType = "code";

	options.Scope.Clear();
	options.Scope.Add("offline_access");
	options.Scope.Add("openid");
	options.Scope.Add("profile");
	options.Scope.Add("email");
	options.Scope.Add("practicemanager");

	options.Events = new OpenIdConnectEvents
	{
		OnTokenValidated = OnTokenValidated()
	};
});
```

You may have also noticed that we're defining a function to be run when tokens are successfully validated. We're using this event to save access/refresh tokens, along with the access token's expiry time, into our token store for the user that has just successfully signed in/up. Below you'll find the implementation of our `OnTokenValidated` function.

```
private static Func<TokenValidatedContext, Task> OnTokenValidated()
{
    return context =>
    {
        var tokenStore = context.HttpContext.RequestServices.GetService<MemoryTokenStore>();

        var token = new XeroOAuth2Token
        {
            AccessToken = context.TokenEndpointResponse.AccessToken,
            RefreshToken = context.TokenEndpointResponse.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(Convert.ToDouble(context.TokenEndpointResponse.ExpiresIn))
        };

        tokenStore.SetToken(context.Principal.XeroUserId(), token);

        return Task.CompletedTask;
    };
}
```

### Enforcing the authentication.

Now that we've configured how we want our users to be authenticated, we've got to enable and enforce it.

Looking at the *configure* method in the same `Startup` class, you'll see the following line of code. **Note**: it's important that this line of code exists before the `UseMvc` line so that the request pipeline can enforce authentication prior to routing the requests to your controllers.

```
app.UseAuthentication();
```

Looking at the *TenantClientLists* route on our [HomeController](XeroPracticeManagerOAuth2Sample/Controllers/HomeController.cs), you'll see that we've set the route to require authorized users using the `Authorize` attribute. This route will use the default scheme (which you'll remember we set earlier) to authenticate the user if the user is not authenticated.

```
[Authorize]
public async Task<IActionResult> TenantClientLists()
...
```

This is different to our *SignIn* route, which defines the specific schemes a user must be authenticated with in our to access the routes


```
[Authorize(AuthenticationSchemes = "XeroSignIn")]
public IActionResult SignIn()
...
```

### Adding "Sign in with Xero" button.
You may have noticed that in the `HomeController` we've left the *Index* route as a non-secure route. We've done this so that we can use this route to present the user with a "[Sign in with Xero](https://developer.xero.com/documentation/oauth2/sign-in#signinbutton)" button.  
Looking at the [home/index](XeroPracticeManagerOAuth2Sample/Views/Home/Index.cshtml) view, you'll find that we've done this by including Xero's pre-built sign in button. The `data-href` attributes in each are linking to our secure route from earlier to kick off the respective signin flow.
```
<span data-xero-sso data-href="/home/signin" data-label="Sign in with Xero"></span>
<script src="https://edge.xero.com/platform/sso/xero-sso.js"></script>
```

### Using the Xero API with OAuth2.0
So, we've got an authenticated user, which means we've got an access token that can be used against Xero's API. But how do we use it?

1. The first thing you're going to want to do is check which tenants have been connected to our app for the authenticated user.  
Looking again at the *TenantClientLists* route on our [HomeController](XeroPracticeManagerOAuth2Sample/Controllers/HomeController.cs) you'll see us retrieve the access token for the user from our token store, and use it to retrieve the connections between the user and our app. We also redirect to a separate page prompting the user to add an organisation if they've signed into the app but never connected an organisation.

```
var token = await _tokenStore.GetAccessTokenAsync(User.XeroUserId());

var connections = await _xeroClient.GetConnectionsAsync(token);

connections = connections.Where(c => c.TenantType == "PRACTICEMANAGER").ToList();

if (!connections.Any())
{
    return RedirectToAction("NoTenants");
}
```

2. If our users do have some connected organisations, we'll loop through them, retrieving the connected organisation's details from Xero's Organisation endpoint, as well as the clients from Xero Practice Manager's Client List endpoint.
```
var data = new List<(Guid tenantId, ClientListResponse clients)>();

var client = _httpClientFactory.CreateClient("XeroPracticeManager");
client.SetBearerToken(token.AccessToken);

//Retrieve the Xero Practice Manager clients for each connection we have access to
foreach (var connection in connections)
{
	ClientListResponse clients = await GetClients(client, connection.TenantId);

	data.Add((connection.TenantId, clients));
}
```

We then build a model to hold this data and present the user with our OutstandingInvoices view.

```
var model = new TenantClientListsModel
            {
                LoggedInUser = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}",
                TenantClients = data
            };

return View(model);
```

## That's it!
1. Browsing to the root of the site will present the Xero sign in button.
2. Clicking the Xero sign in button will route the user to our specific secure route.
3. Microsoft's OIDC implementation kicks in and takes the user through the Xero sign in process.
   1. The user is taken through the Xero sign in process, granting user consent to your app if it's their first time.
      1. The user is also prompted to connect an organisation if you've asked for organisation specific scopes.
   2. The entire OIDC flow is handled for you. No need to swap authorisation codes for access/id/refresh tokens.
   3. The User Principal is set, including all the claims attached to the id token provided by Xero.
   4. Our OnTokenValidated event stores the users access token so it can later be used to access Xero's API.
4. Connections for the user are retrieved using the Connections endpoint and the users access token.
5. Organisation data is retrieved on behalf of the user using their access token, and tenant id from each connection.
6. The user is displayed our secured page with details on their connected organisation's clients.
