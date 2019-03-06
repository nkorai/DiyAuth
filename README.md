# DiyAuth
## What is DiyAuth?
DIY Auth is a project aimed at placing the control and responsibility of user authentication in your hands. It implements a simple authorization and authentication scheme following industry best practices that is plug-and-play to help you jump start your next project. Furthermore it stores your user's authentication data in your cloud instance, allowing you to be free of relying on third party services to manage your data.

## Why is there a need for it?
Everytime I want to start a new side project I invariably have to rebuild the ability to allow users to login. In the past I've used services like Parse (RIP) to do the authentication for me but relying on a centralized third-party service to accomplish your authentication and authorization isn't the best because:

- The service can shut down (Like Parse did) leaving you without a way to authenticate
- It can be a bottleneck in your system that you can't control

It's a decent time sync everytime I have to build it and I'd rather just get to building the meat and potatoes of the idea than building the authentication mechanism everytime. I feel like other developers have the same issue so I decided to build this into an open source library that implements the most common functionality as far as user authentication and authorization goes. 

## Things that you must handle
DiyAuth can handle a lot of the background encrypting and management for you but you must make sure the following things are taken care of:
- You must use HTTPS to ensure the email address and password are secure until they make their way into your application's endpoint
- Do *not* log the password anywhere at any point 

## Recommendations on data layout
The authorization/authentication endpoints in the project return an IdentityId Guid. I would take those and use them in your own databases to store other user related data keyed by this IdentityId e.g. Usernames, Address etc. 

## How do I use it?
First and foremost there is a sample UWP application that uses the DiyAuth project included in the repository in the SampleApp folder.
To use the DiyAuth in it's simplest configuration using an Azure Storage Account as your backend, all you have to do is initialize it as such:
```
var authenticationProvider = await Authenticator.GetAzureAuthenticator(storageAccountConnectionString)
```
I recommend caching the authentication provider somewhere as all other methods are exposed off of it. 

### Standard use cases
The following methods are exposed off of the authentication provider (and are listed in the `IAuthenticationProvider`)
- CheckIdentityExists(string emailAddress)
- CreateIdentity(string emailAddress, string password)
- Authorize(string emailAddress, string password)
- Authenticate(string token)
- ChangePassword(string emailAddress, string oldPassword, string newPassword)
- GenerateTokenForIdentityId(Guid identityId)
- DeleteToken(string token)
- DeleteIdentity(Guid identityId)
