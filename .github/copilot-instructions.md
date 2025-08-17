# Copilot Instructions for FinalSay

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a full-stack voting/polling application built with:
- **Backend**: .NET Core 9 Web API with Entity Framework Core, ASP.NET Identity, and JWT authentication
- **Frontend**: Vue.js 3 with TypeScript, Vue Router, and Axios for API calls
- **Database**: SQL Server (LocalDB for development)

## Architecture
- RESTful API design
- JWT-based authentication
- Entity Framework Code First approach
- Component-based Vue.js frontend
- Responsive design with custom CSS

## Key Features
- User registration and authentication
- Create and manage poll questions
- Vote on questions (Yes/No voting)
- View voting results with percentages
- User profile management
- Real-time vote count updates

## Code Standards
- Use async/await patterns for all database operations
- Follow RESTful API conventions
- Implement proper error handling and validation
- Use TypeScript for type safety in frontend
- Follow Vue 3 Composition API patterns
- Implement proper CORS configuration
- Use JWT tokens for secure API access

## Database Schema
- Users: ApplicationUser (extends IdentityUser)
- Questions: Title, Description, Author, Timestamps
- Votes: User-Question relationship with Yes/No choice
- Unique constraint: One vote per user per question

## Security Considerations
- JWT token authentication
- Password hashing via ASP.NET Identity
- Input validation on both client and server
- CORS configured for frontend origin
- SQL injection protection via Entity Framework

## Development Notes
- Backend runs on https://localhost:5001 (or http://localhost:5000)
- Frontend runs on http://localhost:3000
- Database migrations should be applied before first run
- Environment-specific configuration in appsettings files
