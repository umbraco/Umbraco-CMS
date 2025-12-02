# Error Handling
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---


### Diagnosis Process

1. **Check browser console** for errors and warnings
2. **Reproduce consistently** - Identify exact steps
3. **Check network tab** for failed API calls
4. **Inspect element** to verify DOM structure
5. **Check Lit reactive update cycle** - Use `element.updateComplete`
6. **Verify context availability** - Check context providers
7. **Review event listeners** - Check event propagation
8. **Use browser debugger** with source maps

**Common Web Component Issues**:
- Component not rendering: Check if custom element is defined
- Properties not updating: Verify `@property()` decorator and reactive update cycle
- Events not firing: Check event names and listeners
- Shadow DOM issues: Use `shadowRoot.querySelector()` not `querySelector()`
- Styles not applied: Check Shadow DOM style encapsulation
- Context not available: Ensure context provider is ancestor in DOM tree

### Error Handling Standards

**Error Classes**:

```typescript
// Use built-in Error class or extend it
throw new Error('Failed to load content');

// Custom error classes for domain errors
export class UmbContentNotFoundError extends Error {
	constructor(id: string) {
		super(`Content with id "${id}" not found`);
		this.name = 'UmbContentNotFoundError';
	}
}
```

**Repository Pattern Error Handling**:

```typescript
async requestById(id: string): Promise<{ data?: UmbContentModel; error?: Error }> {
	try {
		const response = await this._apiClient.getById({ id });
		return { data: response.data };
	} catch (error) {
		return { error: error as Error };
	}
}

// Usage
const { data, error } = await repository.requestById('123');
if (error) {
	// Handle error
	console.error('Failed to load content:', error);
	return;
}
// Use data
```

**Observable Error Handling**:

```typescript
this.observe(dataSource$, (value) => {
	// Success handler
	this._data = value;
}).catch((error) => {
	// Error handler
	console.error('Observable error:', error);
});
```

**Promise Error Handling**:

```typescript
// Always use try/catch with async/await
async myMethod() {
	try {
		const result = await this.fetchData();
		return result;
	} catch (error) {
		console.error('Failed to fetch data:', error);
		throw error; // Re-throw or handle
	}
}

// Or use .catch()
this.fetchData()
	.then(result => this.handleResult(result))
	.catch(error => this.handleError(error));
```

### Web Component Error Handling

**Lifecycle Errors**:

```typescript
export class UmbMyElement extends UmbElementMixin(LitElement) {
	constructor() {
		try {
			super();
			// Initialization that might throw
		} catch (error) {
			console.error('Failed to initialize element:', error);
		}
	}

	async connectedCallback() {
		try {
			super.connectedCallback();
			// Async initialization
			await this.loadData();
		} catch (error) {
			console.error('Failed to connect element:', error);
			this._errorMessage = 'Failed to load component';
		}
	}
}
```

**Render Errors**:

```typescript
override render() {
	if (this._error) {
		return html`<umb-error-message .error=${this._error}></umb-error-message>`;
	}

	if (!this._data) {
		return html`<uui-loader></uui-loader>`;
	}

	return html`
		<!-- Normal render -->
	`;
}
```

### Logging Standards

**Console Logging**:

```typescript
// Development only - Remove before production
console.log('Debug info:', data);

// Errors - Kept in production but sanitized
console.error('Failed to load:', error);

// Warnings
console.warn('Deprecated API usage:', method);

// Avoid console.log in production code (ESLint warning)
```

**Custom Logging** (if needed):

```typescript
// Use debug flag for verbose logging
if (this._debug) {
	console.log('[UmbMyComponent]', 'State changed:', this._state);
}
```

**Don't Log Sensitive Data**:
- User credentials
- API tokens
- Personal information (PII)
- Session IDs
- Full error stack traces in production

### Development Environment

- **Source Maps**: Enabled in Vite config for debugging
- **Error Overlay**: Vite provides error overlay in development
- **Hot Module Replacement**: Instant feedback on code changes
- **Detailed Errors**: Full stack traces with source locations
- **TypeScript Checking**: Real-time type checking in IDE

### Production Environment

- **Sanitized Errors**: No stack traces exposed to users
- **User-Friendly Messages**: Show helpful error messages
- **Error Boundaries**: Catch errors at component boundaries
- **Graceful Degradation**: Fallback UI when errors occur
- **Error Reporting**: Log errors to console (browser dev tools)

**Production Error Display**:

```typescript
private _errorMessage?: string;

override render() {
	if (this._errorMessage) {
		return html`
			<uui-box>
				<p class="error">${this._errorMessage}</p>
				<uui-button @click=${this._retry} label="Try Again"></uui-button>
			</uui-box>
		`;
	}
	// ... normal render
}
```

### Context-Specific Error Handling

**HTTP Client Errors** (OpenAPI):

```typescript
try {
	const response = await this._apiClient.getDocument({ id });
	return response.data;
} catch (error) {
	if (error.status === 404) {
		throw new UmbContentNotFoundError(id);
	}
	if (error.status === 403) {
		throw new UmbUnauthorizedError('Access denied');
	}
	throw error;
}
```

**Observable Subscription Errors**:

```typescript
this._subscription = this._dataSource.asObservable().subscribe({
	next: (value) => this._data = value,
	error: (error) => {
		console.error('Observable error:', error);
		this._errorMessage = 'Failed to load data';
	},
	complete: () => console.log('Observable completed'),
});
```


