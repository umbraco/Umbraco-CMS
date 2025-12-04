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

**Sanitize HTML**:

```typescript
// Use DOMPurify for HTML sanitization
import DOMPurify from '@umbraco-cms/backoffice/external/dompurify';

const cleanHtml = DOMPurify.sanitize(userInput);

// In Lit templates, use unsafeHTML directive with sanitized content
import { unsafeHTML } from '@umbraco-cms/backoffice/external/lit';

render() {
	return html`<div>${unsafeHTML(DOMPurify.sanitize(this.htmlContent))}</div>`;
}
```

### Authentication & Authorization

**OpenID Connect** via backend:
- Backoffice uses OpenID Connect for authentication
- Authentication handled by .NET backend
- Tokens managed by browser (httpOnly cookies)

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
- Use Context API for auth state
- Don't store sensitive tokens in localStorage
- Backend handles token refresh

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
// Lit automatically escapes content in templates
render() {
	// Safe - Automatically escaped
	return html`<div>${this.userContent}</div>`;

	// UNSAFE - Only use with sanitized content
	return html`<div>${unsafeHTML(DOMPurify.sanitize(this.htmlContent))}</div>`;
}
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


