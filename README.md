# DiyAuth
## What is DiyAuth?
DIY Auth is a project aimed at placing the control and responsibility of user authentication in your hands. It implements a simple authorization and authentication scheme following industry best practices that is plug-and-play to help you jump start your next project. Furthermore it stores your user's authentication data in your network, allowing you to be free of relying on third party services to manage your data. 

## Why is there a need for it?
Everytime I want to start a new side project I invariably have to rebuild the ability to allow users to login. In the past I've used services like Parse (RIP) to do the authentication for me but relying on a centralized third-party service to accomplish your authentication and authorization isn't the best because:

- The service can shut down (Like Parse did) leaving you without a way to authenticate
- It can be a bottleneck in your system that you can't control

It's a decent time sync everytime I have to build it and I'd rather just get to building the meat and potatoes of the idea than building the authentication mechanism everytime. I feel like other developers have the same issue so I decided to build this into an open source library that implements the most common functionality as far as user authentication and authorization goes. 

## How do I use it?
TODO

## Recommendations on data layout
The authorization/authentication endpoints in the project return an IdentityId Guid. I would take those and use them in your own databases to store other user related data keyed by this IdentityId e.g. Usernames, Address etc. 

## Use cases 


## Customization
