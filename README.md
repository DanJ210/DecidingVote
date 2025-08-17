# FinalSay – Democracy's answer to 'because I said so.'

A full‑stack web application where authenticated users create questions with two custom‑labeled sides (e.g., "Plaintiff" vs "Defense", "Option A" vs "Option B") and community members cast a single vote which they may change within a 24‑hour grace window.

## Features

- **JWT Authentication**: Secure register/login using ASP.NET Identity + JWT
- **Custom Two-Side Questions**: Each question stores `Side1Text` & `Side2Text` (user-defined labels)
- **Single Vote per User per Question**: Enforced at the data layer
- **Vote Change Window**: Users may change their vote within 24 hours of initial cast
- **Live Percentages**: Dynamic progress bars showing side percentages & totals
- **User Profiles**: View authored questions and personal voting activity
- **Accessible UI**: Incremental migration to Tailwind CSS utility + component layers
 - **API Testing Workflow**: Single canonical REST client file (`FinalSay.Api.http`) for rapid endpoint verification
- **Extensible Domain Model**: Enum-based `VoteChoice` allows future side expansions if needed

## Technology Stack

### Backend (.NET 9 Web API)
- **Framework**: ASP.NET Core 9 (Minimal hosting model)
- **Data**: Entity Framework Core (Code First, SQL Server LocalDB for dev)
- **Identity & Auth**: ASP.NET Identity + JWT (HS256, configurable expiry)
- **Object Model**: Questions, Votes, ApplicationUser, enum `VoteChoice { Side1, Side2 }`
- **Validation**: Data annotations + server-side checks
 - **Migrations**: Tracked under `FinalSay.Api/Migrations`

### Frontend (Vue 3 + TypeScript)
- **Framework**: Vue 3 Composition API
- **Tooling**: Vite, TypeScript strict config
- **State/Auth**: Lightweight auth composable (`useAuth`)
- **HTTP**: Axios instance with auth interceptor
- **Routing**: Vue Router
- **Styling**: Tailwind CSS (in progress migration) + legacy CSS fallback
- **Build**: NPM scripts (`dev`, `build`)

### Styling & Design System
- Tailwind utilities enabled with custom color palette (`primary`, `secondary`, `success`, `danger`)
- Planned component layer: semantic classes (e.g., `.btn`, `.card`, `.input`) built atop utilities
- Progressive conversion—legacy selectors removed once unused
 - Dark mode (class-based) with persisted toggle and smooth color transitions

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- SQL Server LocalDB (comes with Visual Studio or SQL Server Express)

### Backend Setup

1. Navigate to the API directory:
   ```bash
   cd FinalSay.Api
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Install Entity Framework Core tools (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Apply existing database migrations (already included in repo):
   ```bash
   dotnet ef database update
   ```

5. Run the API:
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd final-say-frontend
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

## API Endpoints (Current)

### Auth
- `POST /api/auth/register` – Register new user
- `POST /api/auth/login` – Obtain JWT
- `GET /api/auth/profile` – Current user profile (auth)

### Questions
- `GET /api/questions` – List all questions (aggregated vote counts & percentages)
- `GET /api/questions/{id}` – Get a single question
- `POST /api/questions` – Create question with `title`, `description`, `side1Text`, `side2Text` (auth)
- `GET /api/questions/user` – List questions authored by current user (auth)
- `DELETE /api/questions/{id}` – Delete owned question (auth, owner)

### Votes
- `POST /api/votes` – Cast initial vote `{ questionId, choice }` where `choice` ∈ `Side1|Side2` (auth)
- `GET /api/votes/user` – Current user's votes (auth)
- `GET /api/votes/question/{questionId}` – Raw votes for question (primarily diagnostic)
- `GET /api/votes/can-change/{questionId}` – Boolean/time window check for changing current user's vote (auth)
- `PUT /api/votes/change` – Change existing vote within 24h `{ questionId, newChoice }` (auth)

### Notes
- Legacy Yes/No endpoints/fields replaced by enum-based choice model.
- One vote per user per question enforced by unique constraint.

For concrete request examples see `FinalSay.Api.http`.

## Data Model (Current)

### ApplicationUser
Standard ASP.NET Identity user + navigation collections.

### Question
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Title | nvarchar(200) | Required |
| Description | nvarchar(1000) | Optional/trimmed |
| Side1Text | nvarchar(200) | Required (custom label) |
| Side2Text | nvarchar(200) | Required (custom label) |
| UserId | string | FK -> ApplicationUser |
| CreatedAt | DateTime (UTC) | Set server-side |
| Votes | ICollection<Vote> | Navigation |

### Vote
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | string | FK -> ApplicationUser |
| QuestionId | int | FK -> Question |
| Choice | enum `VoteChoice` | `Side1` or `Side2` |
| CreatedAt | DateTime (UTC) | Initial timestamp |
| LastModifiedAt | DateTime? (UTC) | Updated when vote changed |

### Constraints
- Unique composite index: (UserId, QuestionId)
- Vote change permitted only if `UtcNow - CreatedAt < 24h`

## Security & Integrity

- JWT token-based authentication (7-day default token window – configurable)
- Password hashing via ASP.NET Identity
- CORS restricted to frontend origin in dev
- EF Core parameterization prevents SQL injection
- Server-side model validation & DTO shaping
- Unique voting constraint (UserId + QuestionId)
- Vote change restricted by 24h window logic

## Development

### Running Tests
```bash
# Frontend tests
cd final-say-frontend
npm run test:unit

# Backend tests (if implemented)
cd FinalSay.Api
dotnet test
```

### Building for Production
```bash
# Frontend build
cd final-say-frontend
npm run build

# Backend publish
cd FinalSay.Api
dotnet publish -c Release
```

## Configuration

### Backend Configuration (appsettings.json)
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FinalSayDb;Trusted_Connection=true;"
   },
   "Jwt": {
      "Key": "your-secret-key-here",
      "Issuer": "FinalSay",
      "Audience": "FinalSay"
   }
}
```

### Frontend Configuration (vite.config.ts)
Vite dev server can proxy API calls to backend origin when configured; adjust `.env` or axios base URL in `plugins/axios.ts` as needed.

### Tailwind Configuration
`tailwind.config.js` includes content globs for Vue/TS files and extended color palette. Future enhancements: semantic tokens (spacing scale aliases), dark mode toggling, forms plugin.

### Environment Variables
Add a `.env` (frontend) for values like `VITE_API_BASE_URL` if you need to override defaults.

### API Testing
Use the consolidated REST client file `FinalSay.Api.http` (VS Code REST Client extension) for manual exploration. Keep this file authoritative; avoid duplicating per-feature variants.

## Styling Migration & Dark Mode
Completed:
1. Tailwind foundations, custom palette, forms plugin
2. Component layer utilities (`.btn`, `.card`, `.input`, progress/status)
3. Migration of all main views (Questions, Create Poll, Auth, Profile, Home) + layout
4. Dark mode implementation (`dark` class on `<html>` via `useColorMode` composable)
5. Pruned legacy global CSS & removed obsolete selectors

Current Enhancements:
- Dark variants for navigation, cards, inputs, status panels, progress bars, hero sections
- Smooth theme transitions (`transition-colors duration-300` on body & interactive elements)

Remaining / Optional:
1. Extract reusable nav & stat components
2. Add icon-only theme toggle variant
3. Additional semantic tokens (spacing, typography scale aliases)
4. Automated accessibility testing pipeline
5. Consider CSS variable layer for future theming beyond light/dark

### Dark Mode Usage
- A toggle button in the header switches between light/dark.
- Preference persisted in `localStorage` (`color-mode`).
- Initial mode resolves from stored preference else `prefers-color-scheme` media query.
- System preference changes auto‑apply if user hasn't explicitly chosen a mode.
- All core surfaces (background, nav, footer, cards, panels, progress) have dark variants.

## Progressive Web App (PWA) Enablement

Initial PWA support has been added to allow installation and basic offline capability.

What’s Included:
- `public/manifest.webmanifest` with app metadata, theme & background colors, and icon declarations
- Basic service worker `public/sw.js` implementing:
   - Cache-first strategy for static shell assets (`/`, `index.html`, manifest, favicon)
   - Network-first for `/api/` requests (falls back to cache only if offline and previously cached)
- Automatic registration script in `index.html` (executes on window `load`)
- `<meta name="theme-color" content="#3498db">` for address bar coloring on supported browsers

To Finish (Optional but Recommended):
1. Generate real icons at 192x192 & 512x512 (plus other sizes like 256 & maskable) and place under `public/icons/`
2. Add an Apple touch icon & iOS splash assets for improved iOS install experience
3. Provide an offline fallback page (e.g., `offline.html`) and route navigation requests to it when offline
4. Version your cache names (already using `finalsay-static-v1`) and add a lightweight runtime update handshake
5. Consider swapping to a build-time generator (e.g., `@vite-pwa/plugin`) for precache manifest automation if scope grows
6. Monitor service worker life-cycle (optionally postMessage from `sw.js` to notify clients of updates)

Updating the Service Worker:
- Increment `CACHE_NAME` to bust old caches after modifying static assets
- Optionally call `registration.update()` periodically or prompt users to refresh when a new SW is waiting

Limitations (Current Minimal Implementation):
- No offline queueing for write actions (vote submissions while offline will currently fail)
- No dynamic precache of newly added routes beyond first visit
- No background sync or push notifications configured yet

Next Steps for Enhanced PWA UX:
- Add route-level offline fallbacks
- Implement Background Sync for deferred vote submissions
- Integrate push notifications for new questions (requires backend endpoints & user permission flow)
- Add periodic SW sync to refresh question list

Testing PWA Locally:
1. Run `npm run dev` (Vite serves from memory; SW still registers)
2. Open Chrome DevTools > Application > Manifest to verify manifest validity
3. Use Lighthouse (Chrome DevTools) to audit PWA installability and performance
4. Toggle Offline in DevTools Network panel to confirm cached shell renders

Security Note:
- SW intentionally skips caching API POST/PUT/DELETE responses to avoid serving stale mutative data.

If adding advanced caching logic, keep API semantics (freshness over staleness) in mind; prefer stale‑while‑revalidate patterns only for idempotent GET resources.

## Testing & Quality
Lightweight manual verification currently; future additions could include:
- Backend unit tests for vote change window logic
- Frontend component tests (mount & interaction for vote casting)
- Accessibility audits (axe-core) during CI

## Future Enhancements (Roadmap)
- Icon-only / compact theme switcher & advanced theming tokens
- Pagination / infinite scroll for questions
- Real-time updates (SignalR or WebSocket) for vote counts
- Admin moderation tools
- Extended vote change policy (configurable duration)
- Export / shareable results view

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

Open an issue for questions, suggestions, or support.

---
Maintained with an eye toward clarity, testability, and incremental UI modernization.
