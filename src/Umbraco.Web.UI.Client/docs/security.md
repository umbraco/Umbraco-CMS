# Security

[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---

### Input Validation

**Validate All User Input**:

```typescript
// Use validation in forms
import { UmbValidationController } from '@umbraco-cms/backoffice/validation';

#validation = new UmbValidationController(this);

async #handleSubmit() {
	if (!this.#validation.validate()) {
		return; // Show validation errors
	}
	// Proceed with submission
}
```

**String Validation**:

```typescript
private _validateName(name: string): boolean {
	// Length check
	if (name.length === 0 || name.length > 100) {
		return false;
	}

	// Pattern check (example: alphanumeric and spaces)
	if (!/^[a-zA-Z0-9\s]+$/.test(name)) {
		return false;
	}

	return true;
}
```

### Authentication & Authorization

**OpenID Connect with PKCE** (v17+):

- Backoffice uses PKCE authorization code flow
- Real tokens are stored exclusively in `__Host-umbAccessToken` / `__Host-umbRefreshToken` httpOnly cookies — JavaScript cannot read them
- The client-side bearer token value is always the literal string `'[redacted]'` — the server (`HideBackOfficeTokensHandler`) swaps it for the real cookie on each request
- Never try to read or store real tokens client-side; they are intentionally inaccessible

**Configuring API clients**:

```typescript
// ✅ One-liner for @hey-api/openapi-ts clients — handles auth + credentials automatically
authContext.configureClient(myClient);

// ✅ For manual fetch calls
const config = authContext.getOpenApiConfiguration();

// ❌ getLatestToken() is deprecated (returns '[redacted]' anyway) — scheduled for removal v19
const token = await authContext.getLatestToken();
```

**Do NOT call `validateToken()` per request**:
`validateToken()` forces a `/token` network call and revokes the previous access token as a side effect (OpenIddict reference tokens). Calling it on every API request causes token churn and ID2019 "token no longer valid" errors in concurrent requests. Use `configureClient()` instead — it has a built-in guard that only refreshes when the access token is actually expired.

**Authorization Checks**:

```typescript
// Check user permissions before actions
#authContext?: UmbAuthContext;

async #handleDelete() {
	const hasPermission = await this.#authContext?.hasPermission('delete');
	if (!hasPermission) {
		// Show error or hide action
		return;
	}
	// Proceed with deletion
}
```

**Context Security**:

- Use Context API for auth state (`UMB_AUTH_CONTEXT`)
- Never store tokens in localStorage, sessionStorage, or JS variables
- Backend handles token refresh via httpOnly cookies
- All API requests must use `credentials: 'include'` (handled automatically by `configureClient()`)

### API Security

**HTTP Client Security**:

```typescript
// OpenAPI client handles:
// - CSRF tokens
// - Request headers
// - Credentials
// - Error handling

// Use generated OpenAPI client
import { ContentResource } from '@umbraco-cms/backoffice/external/backend-api';

const client = new ContentResource();
const response = await client.getById({ id });
```

**CORS** (Backend Configuration):

- Configured in .NET backend
- Backoffice follows same-origin policy
- API calls to same origin

**Rate Limiting** (Backend):

- Handled by .NET backend
- Backoffice respects rate limit headers

### XSS Prevention

**Template Security** (Lit):

```typescript
import { sanitizeHTML, escapeHTML } from '@umbraco-cms/backoffice/utils';

// Lit automatically escapes content in templates
render() {

	// UNSAFE - This is only safe if you are 100% sure that htmlContent is sanitized properly before being set, and that it cannot be manipulated by user input in any way. Use with extreme caution.
	return html`<div>${unsafeHTML(this.htmlContent)}</div>`;

	// Safe - Automatically escaped
	return html`<div>${this.userContent}</div>`;

	// Safe - Use with sanitized content
	return html`<div>${unsafeHTML(sanitizeHTML(this.htmlContent))}</div>`;

	// Safe - Use with escaped content, which is essentially what Lit does by default
	return html`<div>${unsafeHTML(escapeHTML(this.htmlContent))}</div>`;

	// Safe - localize.htmlString() escapes interpolated args and wraps in unsafeHTML for rendering
	return html`<div>${this.localize.htmlString('#someKey_withHtml', this.userContent)}</div>`;

	// Safe - <umb-localize> component automatically escapes arguments
	return html`<umb-localize key="someKey_withHtml" .args=${[ this.userContent ]}></umb-localize>`;
}
```

**Localized HTML — `localize.string()` vs `localize.htmlString()`**:

- `localize.string(text, ...args)` — returns a plain string. Use for **non-HTML** contexts: attribute bindings (Lit auto-escapes), notification messages, button labels, log strings. Args are NOT escaped because Lit (or the consumer) handles the appropriate escaping for the context.
- `localize.htmlString(text, ...args)` — returns a Lit directive that renders via `unsafeHTML` with all args HTML-escaped. Use whenever the localized value contains HTML markup that must be rendered (e.g. `<a>` links, `<strong>` emphasis) — this is the only safe path when interpolating user-controlled args into HTML output.

```typescript
// ✅ Plain text — string() is correct (Lit escapes the attribute itself)
html`<uui-button label=${this.localize.string('#actions_delete')}></uui-button>`;

// ✅ HTML rendering — htmlString() escapes args + wraps in unsafeHTML
html`<p>${this.localize.htmlString('#defaultdialogs_confirmdelete', userControlledName)}</p>`;

// ❌ Manually combining string() + unsafeHTML leaves args un-escaped — XSS hazard
html`<p>${unsafeHTML(this.localize.string('#defaultdialogs_confirmdelete', userControlledName))}</p>`;
```

**Modal `content` field** (e.g. `umbConfirmModal`) renders strings via `unsafeHTML` internally. When passing a localized string with user-controlled args, wrap it in a template:

```typescript
// ✅ Safe — htmlString escapes args, html`...` wraps the directive in a TemplateResult
umbConfirmModal(this, {
	headline: '#actions_delete',
	content: html`${this.#localize.htmlString('#defaultdialogs_confirmdelete', item.name)}`,
});
```

**Attribute Binding**:

```typescript
// Safe - Lit escapes attribute values
render() {
	return html`<input value=${this.userInput} />`;
}
```

**Event Handlers**:

```typescript
// Safe - Event handlers are not strings
render() {
	return html`<button @click=${this.#handleClick}>Click</button>`;
}

// NEVER do this (code injection risk)
// render() {
//   return html`<button onclick="${this.userCode}">Click</button>`;
// }
```

### Content Security Policy

**CSP Headers** (Backend Configuration):

- Configured in .NET backend
- Restricts script sources
- Prevents inline scripts (except with nonce)
- Reports violations

**Backoffice Compliance**:

- No inline scripts
- No `eval()` or `Function()` constructor
- Monaco Editor uses web workers (CSP compliant)

### Dependencies Security

**Package Management**:

```bash
# Check for vulnerabilities
npm audit

# Fix automatically
npm audit fix

# Update dependencies carefully
npm update
```

**Dependency Security Practices**:

- Renovate bot automatically creates PRs for updates
- Review dependency changes before merging
- Only use packages from npm registry
- Verify package integrity
- Keep dependencies updated

**Known Vulnerabilities**:

- CI checks for vulnerabilities on every PR
- Security advisories reviewed regularly

### Common Vulnerabilities

**XSS (Cross-Site Scripting)**:

- ✅ Lit templates automatically escape content
- ✅ DOMPurify for HTML sanitization
- ❌ Never use `unsafeHTML` with user input directly
- ❌ Never set `innerHTML` with user input

**CSRF (Cross-Site Request Forgery)**:

- ✅ Backend sends CSRF tokens
- ✅ OpenAPI client includes tokens automatically
- ✅ SameSite cookies

**Injection Attacks**:

- ✅ Backend uses parameterized queries
- ✅ Input validation on both frontend and backend
- ✅ OpenAPI client prevents injection

**Prototype Pollution**:

- ❌ Never use `Object.assign` with user input as source
- ❌ Never use `_.merge` with untrusted data
- ✅ Validate object shapes before using

**ReDoS (Regular Expression Denial of Service)**:

- ✅ Review complex regex patterns
- ✅ Test regex with long inputs
- ❌ Avoid backtracking in regex

### Secure Coding Practices

**Don't Trust Client Data**:

- Validate on backend (primary defense)
- Frontend validation is UX, not security

**Principle of Least Privilege**:

- Only request permissions needed
- Check permissions before sensitive operations
- Hide UI for unavailable actions

**Sanitize Output**:

- Always sanitize HTML before rendering
- Escape special characters in user content
- Use Lit's automatic escaping

**Secure Defaults**:

- Forms should validate by default
- Sensitive operations require confirmation
- Errors don't expose sensitive information

**Defense in Depth**:

- Multiple layers of security
- Frontend validation + Backend validation
- Input sanitization + Output escaping
- Authentication + Authorization

### Security Anti-Patterns to Avoid

❌ **Never do this**:

```typescript
// XSS vulnerability
element.innerHTML = userInput;

// Code injection
eval(userCode);

// Exposing sensitive data
console.log('Token:', authToken);

// Storing secrets
localStorage.setItem('apiKey', key);

// Disabling validation
// @ts-ignore
// eslint-disable-next-line

// Trusting user input
const url = userInput; // Could be javascript:alert()
window.location.href = url;
```

✅ **Do this instead**:

```typescript
// Safe HTML rendering
element.textContent = userInput;
// or
render() {
	return html`<div>${userInput}</div>`;
}

// No eval needed
// Use proper JavaScript patterns

// Don't log sensitive data
console.log('Operation completed');

// Backend manages secrets
// Frontend receives tokens via httpOnly cookies

// Fix TypeScript/ESLint issues properly
// Don't suppress warnings

// Validate and sanitize URLs
const url = new URL(userInput, window.location.origin);
if (url.protocol === 'https:' || url.protocol === 'http:') {
	window.location.href = url.href;
}
```
