# VotingApp - Polling and Voting System

A full-stack web application that allows users to create polls/questions and vote on them with a simple Yes/No choice system.

## Features

- **User Authentication**: Register and login with JWT token-based authentication
- **Create Polls**: Authenticated users can create questions with title and description
- **Vote on Questions**: Users can vote Yes or No on any question (one vote per user per question)
- **View Results**: Real-time voting results with percentages and vote counts
- **User Profiles**: View personal statistics and created questions
- **Responsive Design**: Works on desktop and mobile devices

## Technology Stack

### Backend (.NET Core 9 Web API)
- **Framework**: ASP.NET Core 9
- **Database**: Entity Framework Core with SQL Server LocalDB
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Architecture**: RESTful API with Controllers

### Frontend (Vue.js 3)
- **Framework**: Vue.js 3 with TypeScript
- **Routing**: Vue Router
- **HTTP Client**: Axios
- **Build Tool**: Vite
- **Styling**: Custom CSS with responsive design

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- SQL Server LocalDB (comes with Visual Studio or SQL Server Express)

### Backend Setup

1. Navigate to the API directory:
   ```bash
   cd VotingApp.Api
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Update the database:
   ```bash
   dotnet ef database update
   ```
   (If no migrations exist, create one first: `dotnet ef migrations add InitialCreate`)

4. Run the API:
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd voting-app-frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```
   The frontend will be available at `http://localhost:3000`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/profile` - Get user profile (requires auth)

### Questions
- `GET /api/questions` - Get all questions
- `GET /api/questions/{id}` - Get specific question
- `POST /api/questions` - Create new question (requires auth)
- `GET /api/questions/user` - Get current user's questions (requires auth)
- `DELETE /api/questions/{id}` - Delete question (requires auth, owner only)

### Votes
- `POST /api/votes` - Cast a vote (requires auth)
- `GET /api/votes/user` - Get current user's votes (requires auth)
- `GET /api/votes/question/{questionId}` - Get votes for specific question
- `DELETE /api/votes/{id}` - Delete vote (requires auth, owner only)

## Database Schema

### Users (ApplicationUser)
- Id (string, PK)
- Username (string, unique)
- Email (string, unique)
- CreatedAt (DateTime)
- + ASP.NET Identity fields

### Questions
- Id (int, PK)
- Title (string, max 200 chars)
- Description (string, max 1000 chars)
- UserId (string, FK to Users)
- CreatedAt (DateTime)

### Votes
- Id (int, PK)
- UserId (string, FK to Users)
- QuestionId (int, FK to Questions)
- IsYes (boolean)
- CreatedAt (DateTime)
- Unique constraint on (UserId, QuestionId)

## Security Features

- JWT token-based authentication
- Password hashing via ASP.NET Core Identity
- CORS configuration for frontend
- SQL injection protection via Entity Framework
- Input validation on both client and server
- Unique voting constraint (one vote per user per question)

## Development

### Running Tests
```bash
# Frontend tests
cd voting-app-frontend
npm run test:unit

# Backend tests (if implemented)
cd VotingApp.Api
dotnet test
```

### Building for Production
```bash
# Frontend build
cd voting-app-frontend
npm run build

# Backend publish
cd VotingApp.Api
dotnet publish -c Release
```

## Configuration

### Backend Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VotingAppDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "VotingApp",
    "Audience": "VotingApp"
  }
}
```

### Frontend Configuration (vite.config.ts)
The frontend is configured to proxy API requests to the backend during development.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions or support, please open an issue in the GitHub repository.
