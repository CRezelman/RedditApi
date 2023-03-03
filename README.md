# RedditApi
Makeshift Reddit API

## Environment
This Restful API is developed and maintained using .NET 7 using Rider IDE. Although other IDEs can be used, everything was tested using Rider.

## Database
This project makes use of an in memory database context and therefore the data only persists while the project is running. Take note restarting the application will result in all data to be lost. The primary function is to verify the endpoints work as expected, and this can be achieved with the current setup.

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
Authorisation can also be done using Swagger by inserting `bearer <Your JWT>` into the authorise field.

## Posts
A user can create as many posts as they like but only the owner of the post of edit or delete their posts. 

## Comments
A user can comment on any post that exists and only the owner of the comment can edit or delete their comments. There is no limit to the number of comments allowed per post.

## Ratings
Ratings follow the folling scheme:
0: None
1: Downvote
2: Upvote

Anyone can rate a post or a comment once, they cannot delete their rating but can change it to any valid state in the scheme. 

## Testing
Testing was performed using swagger at `/swagger/index.html`. All endpoints were tested and to make sure they behaved as expected.

### Endpoints available:
- `/api/Post` GET, POST
- `/api/Post/{id}` GET, PUT, DELETE
- `/api/Post/Rating/{id}` GET
- `/api/Post/{idPost}/Rating/{id}` PUT
- `/api/Post/{idPost}/Rating` POST
- `/api/Comment` GET, POST
- `/api/Comment/{id}` GET, PUT, DELETE