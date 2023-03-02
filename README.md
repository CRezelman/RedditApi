# RedditApi
Makeshift Reddit API

## Environment
This Restful API is developed and maintained using .NET 7 using Rider IDE. Although other IDEs can be used, everything was tested using Rider.

## Authentication

### Registering
Any user can sign up for access to the API using the `/api/Auth/register` endpoint by provinding a username and a password in the body with the following structure:

```json
{
  "username": "string",
  "password": "string"
}
```
Passwords are hashed and stored as such. The endpoint will return a userID, username and hashed password.

### Login
Once you have registered you will need to login with the `/api/Auth/login` endpoint and provide your login details that you registered with by adding the following object to the body:

```json
{
  "username": "string",
  "password": "string"
}
```
If successful, the endpoint will return a JWT that needs to be added in the header for any endpoint that requires authorisation.


## Authorisation
Endpoints that require authorisation will return a 401 error if no authorisation is provided or if an invalid token is provided.
The following header needs to provided when utilising endpoints that require authorisation:
```
"Authorisation": "bearer <Your JWT>"
```