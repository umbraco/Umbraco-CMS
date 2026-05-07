# Umbraco.Web.UI.Login

TypeScript/Lit login SPA for Umbraco CMS backoffice authentication. Provides the `<umb-auth>` web component used in the login page, supporting local login, MFA, password reset, and user invitation flows.

**Project Type**: TypeScript Library (Vite)
**Runtime**: Node.js (see `.nvmrc`), npm >= 10.9
**Output**: ES Module library → `../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login/`
**Dependencies**: @umbraco-cms/backoffice, Lit, Vite

---

## 1. Architecture

### Project Purpose

This project builds the login single-page application:

1. **Web Component** - `<umb-auth>` custom element for authentication
2. **Authentication Flows** - Login, MFA, password reset, user invitation
3. **Localization** - Multi-language support (en, de, da, nb, nl, sv)
4. **API Integration** - Direct `fetch()` for login/MFA; `SecurityService`/`UserService` from `@umbraco-cms/backoffice/external/backend-api` for other operations

### Folder Structure

```
Umbraco.Web.UI.Login/
├── src/
│   ├── components/
│   │   ├── layouts/                  # Layout components
│   │   │   ├── auth-layout.element.ts
│   │   │   ├── confirmation-layout.element.ts
│   │   │   ├── error-layout.element.ts
│   │   │   └── new-password-layout.element.ts
│   │   ├── pages/                    # Page components
│   │   │   ├── login.page.element.ts
│   │   │   ├── mfa.page.element.ts
│   │   │   ├── new-password.page.element.ts
│   │   │   ├── reset-password.page.element.ts
│   │   │   └── invite.page.element.ts
│   │   └── back-to-login-button.element.ts
│   ├── contexts/
│   │   ├── auth.context.ts           # Authentication state
│   │   └── auth.repository.ts        # API calls
│   ├── controllers/
│   │   └── slim-backoffice-initializer.ts  # Minimal backoffice bootstrap
│   ├── localization/
│   │   ├── lang/                     # Language files (da, de, en, en-us, nb, nl, sv)
│   │   └── manifests.ts              # Localization registration
│   ├── mocks/                        # MSW mock handlers for development
│   │   ├── handlers/
│   │   │   ├── login.handlers.ts
│   │   │   └── backoffice.handlers.ts
│   │   └── data/login.data.ts
│   ├── utils/
│   │   └── load-custom-view.function.ts
│   ├── auth.element.ts               # Main <umb-auth> component
│   ├── types.ts                      # Type definitions
│   ├── manifests.ts                  # Extension manifests
│   └── umbraco-package.ts            # Package definition
├── public/
│   └── favicon.svg
├── index.html                        # Development entry point
├── package.json                      # npm configuration
├── tsconfig.json                     # TypeScript config
├── vite.config.ts                    # Vite build config
├── .nvmrc                            # Node version pin
└── .prettierrc.json                  # Prettier config
```

### Build Output

Built assets are output to `Umbraco.Cms.StaticAssets`:
```
../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login/
├── login.js         # Main ES module
└── login.js.map     # Source map
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

### Development

```bash
cd src/Umbraco.Web.UI.Login

# Install dependencies
npm install

# Run development server with hot reload
npm run dev

# Build for production (outputs to StaticAssets)
npm run build

# Watch mode build
npm run watch

# Preview production build
npm run preview
```

---

## 3. Key Components

### UmbAuthElement (src/auth.element.ts)

Main `<umb-auth>` web component.

**Attributes**:
| Attribute | Type | Description |
|-----------|------|-------------|
| `disable-local-login` | boolean | Disables local login, external only |
| `background-image` | string | Login page background URL |
| `logo-image` | string | Logo URL |
| `logo-image-alternative` | string | Alternative logo (dark mode) |
| `username-is-email` | boolean | Use email as username |
| `allow-password-reset` | boolean | Show password reset link |
| `allow-user-invite` | boolean | Enable user invitation flow |
| `return-url` | string | Redirect URL after login |

**Authentication Flows**:
- Default: Login page with username/password
- `flow=mfa`: Multi-factor authentication
- `flow=reset`: Request password reset
- `flow=reset-password`: Set new password
- `flow=invite-user`: User invitation acceptance

**Form Workaround**:
Creates login form in light DOM (not shadow DOM) to support Chrome autofill/autocomplete, which doesn't work properly with shadow DOM inputs.

### UmbAuthContext (src/contexts/auth.context.ts)

Authentication state and API methods:
- `login(data)` - Authenticate user
- `resetPassword(username)` - Request password reset
- `validatePasswordResetCode(userId, code)` - Validate reset code
- `newPassword(password, code, userId)` - Set new password
- `validateInviteCode(token, userId)` - Validate invitation
- `newInvitedUserPassword(...)` - Set invited user password
- `validateMfaCode(code, provider)` - MFA validation

**Return Path Security**: Validates return URL to prevent open redirect attacks - only allows same-origin URLs.

### Localization

Supported languages: English (en, en-us), Danish (da), German (de), Norwegian Bokmål (nb), Dutch (nl), Swedish (sv).

---

## 4. Development with Mocks

### MSW (Mock Service Worker)

Development uses MSW for API mocking. Run `npm run dev` to start with mocks enabled.

**Mock Files**:
- `handlers/login.handlers.ts` - Login, MFA, password reset
- `handlers/backoffice.handlers.ts` - Backoffice API
- `data/login.data.ts` - Test user data
- `customViews/` - Example custom login views

---

## 5. Project-Specific Notes

### Shadow DOM Limitation

Chrome doesn't support autofill in shadow DOM inputs. The login form is created in light DOM via `#initializeForm()` and inserted with `insertAdjacentElement()`. See Chromium intent-to-ship discussion linked in code.

### Slim Backoffice Controller

`UmbSlimBackofficeController` provides minimal backoffice utilities (extension registry, localization, context API) without loading the full app.

### Localization Wait Pattern

Form waits for localization availability before rendering. Retries 40 times with 50ms interval (2 second max wait).

### External Dependencies

- `@umbraco-cms/backoffice` - Umbraco UI components, Lit element base, backoffice API services
- `vite` - Build tooling
- `typescript` - Type checking
- `msw` - API mocking (dev only)

### Known Technical Debt

1. **UUI Color Variable** - Multiple layout/page components use `--uui-color-text-alt` with TODO to change when UUI adds a muted text variable (grep for `--uui-color-text-alt`).

### TypeScript Configuration

Key settings in `tsconfig.json`:
- `target`: ES2022
- `experimentalDecorators`: true (for Lit decorators)
- `useDefineForClassFields`: false (Lit compatibility)
- `moduleResolution`: bundler (Vite)
- `strict`: true

---

## Quick Reference

### Essential Commands

```bash
# Development
npm run dev        # Start dev server
npm run build      # Build to StaticAssets
npm run watch      # Watch mode
```

### Key Files

| File | Purpose |
|------|---------|
| `src/auth.element.ts` | Main `<umb-auth>` component |
| `src/contexts/auth.context.ts` | Auth state management |
| `src/contexts/auth.repository.ts` | API calls |
| `vite.config.ts` | Build configuration |
| `package.json` | Dependencies and scripts |

### Build Output

Built files go to:
```
../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login/
```

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Cms.StaticAssets` | Receives built output |
| `Umbraco.Web.UI.Client` | Backoffice SPA (similar architecture) |
| `@umbraco-cms/backoffice` | NPM dependency for UI components |
