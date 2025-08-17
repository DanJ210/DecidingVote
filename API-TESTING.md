# FinalSay API Quick Testing Guide

This guide shows how to query the API and test authentication using curl and PowerShell on Windows. Default API base URL: http://localhost:5244

## 1. Health/Smoke
- Get questions (public)
  - Method: GET
  - URL: http://localhost:5244/api/questions

## 2. Register
- Method: POST
- URL: http://localhost:5244/api/auth/register
- Body (JSON):
  {
    "username": "alice",
    "email": "alice@example.com",
    "password": "Passw0rd1"
  }
- Notes:
  - Username 3-50 chars, allowed: letters, numbers, . _ @ + -
  - Password >= 6 chars and must include at least one digit

PowerShell
$body = @{ username='alice'; email='alice@example.com'; password='Passw0rd1' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5244/api/auth/register' -ContentType 'application/json' -Body $body

curl
curl -s -X POST http://localhost:5244/api/auth/register -H "Content-Type: application/json" -d '{"username":"alice","email":"alice@example.com","password":"Passw0rd1"}'

Response: { token, username, email }

## 3. Login
- Method: POST
- URL: http://localhost:5244/api/auth/login
- Body (JSON):
  {
    "email": "alice@example.com",
    "password": "Passw0rd1"
  }

PowerShell
$body = @{ email='alice@example.com'; password='Passw0rd1' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5244/api/auth/login' -ContentType 'application/json' -Body $body

curl
curl -s -X POST http://localhost:5244/api/auth/login -H "Content-Type: application/json" -d '{"email":"alice@example.com","password":"Passw0rd1"}'

Response: { token, username, email }

## 4. Use the token (Authorization header)
Save the `token` value from register/login and include it in subsequent requests:
- Header: Authorization: Bearer <token>

PowerShell example (set a variable)
$token = '<paste token here>'
$headers = @{ Authorization = "Bearer $token" }

curl example
-H "Authorization: Bearer <token>"

## 5. Authenticated endpoints

Profile
- GET http://localhost:5244/api/auth/profile
- Requires Authorization header

PowerShell
Invoke-RestMethod -Method Get -Uri 'http://localhost:5244/api/auth/profile' -Headers $headers

curl
curl -s http://localhost:5244/api/auth/profile -H "Authorization: Bearer $token"

Create question with custom voting options
- POST http://localhost:5244/api/questions
- Body (JSON): { "title": "Pizza vs Tacos?", "description": "What should we have for dinner?", "side1Text": "Pizza Night", "side2Text": "Taco Tuesday" }

PowerShell
$body = @{ title='Pizza vs Tacos?'; description='What should we have for dinner?'; side1Text='Pizza Night'; side2Text='Taco Tuesday' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5244/api/questions' -ContentType 'application/json' -Body $body -Headers $headers

curl
curl -s -X POST http://localhost:5244/api/questions -H "Content-Type: application/json" -H "Authorization: Bearer $token" -d '{"title":"Pizza vs Tacos?","description":"What should we have for dinner?","side1Text":"Pizza Night","side2Text":"Taco Tuesday"}'

Get your questions
- GET http://localhost:5244/api/questions/user
Invoke-RestMethod -Method Get -Uri 'http://localhost:5244/api/questions/user' -Headers $headers

Voting (choice 1 or 2)
- POST http://localhost:5244/api/votes
- Body (JSON): { "questionId": 1, "choice": 1 }

PowerShell
$body = @{ questionId=1; choice=1 } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5244/api/votes' -ContentType 'application/json' -Body $body -Headers $headers

Check if vote can be changed (within 24 hours)
- GET http://localhost:5244/api/votes/can-change/1

PowerShell
Invoke-RestMethod -Method Get -Uri 'http://localhost:5244/api/votes/can-change/1' -Headers $headers

Change vote (within 24 hours of original vote)
- PUT http://localhost:5244/api/votes/change
- Body (JSON): { "questionId": 1, "choice": 2 }

PowerShell
$body = @{ questionId=1; choice=2 } | ConvertTo-Json
Invoke-RestMethod -Method Put -Uri 'http://localhost:5244/api/votes/change' -ContentType 'application/json' -Body $body -Headers $headers

Your votes
- GET http://localhost:5244/api/votes/user
Invoke-RestMethod -Method Get -Uri 'http://localhost:5244/api/votes/user' -Headers $headers

Question vote summary (public)
- GET http://localhost:5244/api/votes/question/1

## 6. Common 401 causes and tips
- Missing Authorization header: ensure -Headers $headers or -H "Authorization: Bearer ..." is added.
- Expired token or system clock skew: log in again; ensure your system clock is correct.
- Wrong base URL on frontend: Vite proxy should target http://localhost:5244; axios baseURL is /api.
- Backend not running: start the API; ensure it’s listening on http://localhost:5244.

## 7. Quick frontend testing
Frontend dev server: http://localhost:3000
- It proxies /api to http://localhost:5244
- The app stores token in localStorage under authToken and adds Authorization automatically for API calls.

## 8. Troubleshooting on the server (optional)
The API logs incoming headers for /api/votes and POST /api/questions and prints JWT auth failures during development. Watch the API console when testing.

## 9. VS Code REST Client (recommended)
You can run all of the above requests directly from VS Code using the REST Client extension.

Steps:
- Install the extension: Humao.rest-client
- Open file: FinalSay.Api/FinalSay.Api.http
- Click “Send Request” above each request block.
- First, send the “Login and capture token” request. It saves the token to a {{token}} variable.
- Then run any authenticated requests; they automatically use Authorization: Bearer {{token}}.

Notes:
- The .http file defines @host, @json and a fallback @token. The login request overwrites the global token variable via a script block.
- If you want to test with a different user, just re-run the login request.

