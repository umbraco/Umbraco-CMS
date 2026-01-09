# Umbraco.Web.UI.Login

TypeScript/Lit login SPA for Umbraco CMS backoffice authentication. Provides the `<umb-auth>` web component used in the login page, supporting local login, MFA, password reset, and user invitation flows.

**Project Type**: TypeScript Library (Vite)
**Runtime**: Node.js >= 22, npm >= 10.9
**Output**: ES Module library → `../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login/`
**Dependencies**: @umbraco-cms/backoffice, Lit, Vite

---

## 1. Architecture

### Project Purpose

This project builds the login single-page application:

1. **Web Component** - `<umb-auth>` custom element for authentication
2. **Authentication Flows** - Login, MFA, password reset, user invitation
3. **Localization** - Multi-language support (en, de, da, nb, nl, sv)
4. **API Client** - Generated from OpenAPI specification

### Folder Structure

```
Umbraco.Web.UI.Login/
├── src/
│   ├── api/                          # Generated API client
│   │   ├── client/                   # HTTP client utilities
│   │   ├── core/                     # Serializers, types
│   │   ├── sdk.gen.ts                # Generated SDK
│   │   └── types.gen.ts              # Generated types
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
│   │   ├── auth.context.ts           # Authentication state (86 lines)
│   │   └── auth.repository.ts        # API calls (231 lines)
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
│   │   ├── is-problem-details.function.ts
│   │   └── load-custom-view.function.ts
│   ├── auth.element.ts               # Main <umb-auth> component (404 lines)
│   ├── types.ts                      # Type definitions
│   ├── manifests.ts                  # Extension manifests
│   └── umbraco-package.ts            # Package definition
├── public/
│   └── favicon.svg
├── index.html                        # Development entry point
├── package.json                      # npm configuration (29 lines)
├── tsconfig.json                     # TypeScript config (25 lines)
├── vite.config.ts                    # Vite build config (20 lines)
├── .nvmrc                            # Node version (22)
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

### API Generation

```bash
# Regenerate API client from OpenAPI spec
npm run generate:server-api
```

Uses `@hey-api/openapi-ts` to generate TypeScript client from the Management API OpenAPI specification.

---

## 3. Key Components

### UmbAuthElement (src/auth.element.ts)

Main `<umb-auth>` web component (404 lines).

**Attributes** (lines 172-204):
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

**Authentication Flows** (lines 379-396):
- Default: Login page with username/password
- `flow=mfa`: Multi-factor authentication
- `flow=reset`: Request password reset
- `flow=reset-password`: Set new password
- `flow=invite-user`: User invitation acceptance

**Form Workaround** (lines 274-334):
Creates login form in light DOM (not shadow DOM) to support Chrome autofill/autocomplete, which doesn't work properly with shadow DOM inputs.

### UmbAuthContext (src/contexts/auth.context.ts)

Authentication state and API methods (86 lines):
- `login(data)` - Authenticate user
- `resetPassword(username)` - Request password reset
- `validatePasswordResetCode(userId, code)` - Validate reset code
- `newPassword(password, code, userId)` - Set new password
- `validateInviteCode(token, userId)` - Validate invitation
- `newInvitedUserPassword(...)` - Set invited user password
- `validateMfaCode(code, provider)` - MFA validation

**Return Path Security** (lines 37-54): Validates return URL to prevent open redirect attacks - only allows same-origin URLs.

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

Chrome doesn't support autofill in shadow DOM inputs. The login form is created in light DOM via `#initializeForm()` (lines 274-334) and inserted with `insertAdjacentElement()`. See Chromium intent-to-ship discussion linked in code.

### Slim Backoffice Controller

`UmbSlimBackofficeController` provides minimal backoffice utilities (extension registry, localization, context API) without loading the full app.

### Localization Wait Pattern (lines 242-265)

Form waits for localization availability before rendering. Retries 40 times with 50ms interval (2 second max wait).

### External Dependencies

- `@umbraco-cms/backoffice` ^16.2.0 - Umbraco UI components, Lit element base
- `vite` ^7.2.0 - Build tooling
- `typescript` ^5.9.3 - Type checking
- `msw` ^2.11.3 - API mocking
- `@hey-api/openapi-ts` ^0.85.0 - API client generation

### Known Technical Debt

1. **UUI Color Variable** - Multiple files use `--uui-color-text-alt` with TODO to change when UUI adds muted text variable:
   - `back-to-login-button.element.ts:35`
   - `confirmation-layout.element.ts:41`
   - `error-layout.element.ts:42`
   - `login.page.element.ts:203,226`
   - `new-password-layout.element.ts:221`
   - `reset-password.page.element.ts:110`

2. **API Client Error Types** (`api/client/client.gen.ts:207`): Error handling and types need improvement

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

# API
npm run generate:server-api  # Regenerate API client
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
