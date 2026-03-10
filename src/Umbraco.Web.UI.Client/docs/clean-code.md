# Clean Code
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---


### Function Design

**Function Length**:
- Target: ≤30 lines per function
- Max: 50 lines (enforce via code review)
- Extract complex logic to separate functions
- Use early returns to reduce nesting

**Single Responsibility**:
```typescript
// Good - Single responsibility
private _validateName(name: string): boolean {
	return name.length > 0 && name.length <= 100;
}

private _sanitizeName(name: string): string {
	return name.trim().replace(/[<>]/g, '');
}

// Bad - Multiple responsibilities
private _processName(name: string): { valid: boolean; sanitized: string } {
	// Doing too much in one function
}
```

**Descriptive Names**:
- Use verb names for functions: `loadData`, `validateInput`, `handleClick`
- Boolean functions: `is`, `has`, `can`, `should` prefix
- Event handlers: `handle`, `on` prefix: `handleSubmit`, `onClick`

**Parameters**:
- Limit to 3-4 parameters
- Use options object for more parameters:

```typescript
// Good - Options object
interface LoadOptions {
	id: string;
	includeChildren?: boolean;
	depth?: number;
	culture?: string;
}

private _load(options: LoadOptions) { }

// Bad - Too many parameters
private _load(id: string, includeChildren: boolean, depth: number, culture: string) { }
```

**Early Returns**:

```typescript
// Good - Early returns reduce nesting
private _validate(): boolean {
	if (!this._data) return false;
	if (!this._data.name) return false;
	if (this._data.name.length === 0) return false;
	return true;
}

// Bad - Nested conditions
private _validate(): boolean {
	if (this._data) {
		if (this._data.name) {
			if (this._data.name.length > 0) {
				return true;
			}
		}
	}
	return false;
}
```

### Class Design

**Single Responsibility**:
- Each class should have one reason to change
- Controllers handle one aspect of behavior
- Repositories handle one entity type
- Components handle one UI concern

**Small Classes**:
- Target: <300 lines per class
- Extract complex logic to separate controllers/utilities
- Use composition over inheritance

**Encapsulation**:

```typescript
export class UmbMyElement extends LitElement {
	// Public API - reactive properties
	@property({ type: String })
	value = '';

	// Private state
	@state()
	private _loading = false;

	// Private fields (not reactive)
	#controller = new UmbMyController(this);

	// Private methods
	private _loadData() { }
}
```

### SOLID Principles (Adapted for TypeScript/Lit)

**S - Single Responsibility**:
- One component = one UI responsibility
- One controller = one behavior responsibility
- One repository = one entity type

**O - Open/Closed**:
- Extend via composition and mixins
- Extension API for plugins
- Avoid modifying existing components, create new ones

**L - Liskov Substitution**:
- Subclasses should honor base class contracts
- Use interfaces for polymorphism

**I - Interface Segregation**:
- Small, focused interfaces
- Use TypeScript `interface` for contracts

**D - Dependency Inversion**:
- Depend on abstractions (interfaces) not concrete classes
- Use Context API for dependency injection
- Controllers receive dependencies via constructor

### Dependency Injection

**Context API** (Preferred):

```typescript
export class UmbMyElement extends UmbElementMixin(LitElement) {
	#authContext?: UmbAuthContext;

	constructor() {
		super();

		// Consume context (dependency injection)
		this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;
		});
	}
}
```

**Controller Pattern**:

```typescript
export class UmbMyController extends UmbControllerBase {
	#repository: UmbContentRepository;

	constructor(host: UmbControllerHost, repository: UmbContentRepository) {
		super(host);
		this.#repository = repository;
	}
}

// Usage
#controller = new UmbMyController(this, new UmbContentRepository());
```

### Event Listener Cleanup Pattern

**Critical for Memory Leak Prevention**:

When adding event listeners, you must ensure they can be properly removed. Using `.bind(this)` creates a new function reference each time, making cleanup impossible.

**Wrong Pattern - Memory Leak**:

```typescript
export class UmbMyElement extends LitElement {
	constructor() {
		super();
		// ❌ BAD: Each .bind(this) creates a NEW function reference
		window.addEventListener('storage', this.#onStorageEvent.bind(this));
	}

	disconnectedCallback() {
		// ❌ This creates ANOTHER function reference - doesn't remove the original listener!
		window.removeEventListener('storage', this.#onStorageEvent.bind(this));
	}

	#onStorageEvent(evt: StorageEvent) {
		// Handler logic
	}
}
```

**Correct Pattern - Arrow Function Property**:

```typescript
export class UmbMyElement extends LitElement {
	// ✅ GOOD: Arrow function property maintains consistent reference
	#onStorageEvent = async (evt: StorageEvent) => {
		// Handler logic
		if (evt.key === 'myKey') {
			await this.handleStorageChange();
		}
	};

	constructor() {
		super();
		// ✅ GOOD: No .bind() needed - arrow function preserves 'this'
		window.addEventListener('storage', this.#onStorageEvent);
	}

	disconnectedCallback() {
		// ✅ GOOD: Removes the SAME function reference
		window.removeEventListener('storage', this.#onStorageEvent);
	}
}
```

**Key Points**:
- Use arrow function properties for event handlers
- Arrow functions automatically bind `this` without creating new references
- Each `addEventListener` and `removeEventListener` must use the SAME function reference
- Always remove event listeners in `disconnectedCallback()` for custom elements
- This pattern prevents memory leaks by ensuring proper cleanup

**Common Event Listener Scenarios**:

```typescript
// Document/window events
document.addEventListener('dragenter', this.#onDragEnter);
window.addEventListener('storage', this.#onStorage);

// Programmatically created elements
this.#element = new MyElement();
this.#element.addEventListener('action-executed', this.#onAction);

// Always clean up in disconnectedCallback
disconnectedCallback() {
	document.removeEventListener('dragenter', this.#onDragEnter);
	window.removeEventListener('storage', this.#onStorage);
	if (this.#element) {
		this.#element.removeEventListener('action-executed', this.#onAction);
	}
}
```

### Avoid Code Smells

**Magic Numbers/Strings**:

```typescript
// Bad
if (status === 200) { }
if (type === 'document') { }

// Good
const HTTP_OK = 200;
const CONTENT_TYPE_DOCUMENT = 'document';

if (status === HTTP_OK) { }
if (type === CONTENT_TYPE_DOCUMENT) { }

// Or use enums
enum ContentType {
	Document = 'document',
	Media = 'media',
}
```

**Long Parameter Lists**:

```typescript
// Bad
function create(name: string, type: string, parent: string, culture: string, template: string) { }

// Good
interface CreateOptions {
	name: string;
	type: string;
	parent?: string;
	culture?: string;
	template?: string;
}

function create(options: CreateOptions) { }
```

**Duplicate Code**:
- Extract to shared functions
- Use composition and mixins
- Create utility modules

**Deeply Nested Code**:
- Use early returns
- Extract to separate functions
- Use guard clauses

**Callback Hell** (N/A - use async/await)

### Modern Patterns

**Async/Await**:

```typescript
// Good - Clean async code
async loadContent() {
	try {
		this._loading = true;
		const { data, error } = await this.repository.requestById(this.id);
		if (error) {
			this._error = error;
			return;
		}
		this._content = data;
	} finally {
		this._loading = false;
	}
}
```

**Optional Chaining**:

```typescript
// Good - Safe property access
const name = this._content?.variants?.[0]?.name;

// Bad - Manual null checks
const name = this._content && this._content.variants &&
	this._content.variants[0] && this._content.variants[0].name;
```

**Destructuring**:

```typescript
// Good - Destructure for clarity
const { name, description, icon } = this._content;

// Good - With defaults
const { name = 'Untitled', description = '' } = this._content;
```

**Immutability**:

```typescript
// Good - Spread operator for immutability
this._items = [...this._items, newItem];
this._config = { ...this._config, newProp: value };

// Bad - Mutation
this._items.push(newItem);
this._config.newProp = value;
```

**Pure Functions**:

```typescript
// Good - Pure function (no side effects)
function calculateTotal(items: Item[]): number {
	return items.reduce((sum, item) => sum + item.price, 0);
}

// Bad - Impure (modifies input)
function calculateTotal(items: Item[]): number {
	items.sort((a, b) => a.price - b.price); // Mutation!
	return items.reduce((sum, item) => sum + item.price, 0);
}
```

### TypeScript-Specific Patterns

**Type Guards**:

```typescript
function isContentModel(value: unknown): value is UmbContentModel {
	return typeof value === 'object' && value !== null && 'id' in value;
}

// Usage
if (isContentModel(data)) {
	// TypeScript knows data is UmbContentModel
	console.log(data.id);
}
```

**Discriminated Unions**:

```typescript
type Result<T> =
	| { success: true; data: T }
	| { success: false; error: Error };

function handleResult<T>(result: Result<T>) {
	if (result.success) {
		console.log(result.data); // TypeScript knows this exists
	} else {
		console.error(result.error); // TypeScript knows this exists
	}
}
```

**Utility Types**:

```typescript
// Partial - Make all properties optional
type PartialContent = Partial<UmbContentModel>;

// Pick - Select specific properties
type ContentSummary = Pick<UmbContentModel, 'id' | 'name' | 'icon'>;

// Omit - Remove specific properties
type ContentWithoutId = Omit<UmbContentModel, 'id'>;

// Readonly - Make immutable
type ReadonlyContent = Readonly<UmbContentModel>;
```

### Comments and Documentation

**When to Comment**:
- Explain "why" not "what"
- Document complex algorithms
- JSDoc for public APIs
- Warn about gotchas or non-obvious behavior

**JSDoc for Web Components**:

```typescript
/**
 * A button component that triggers document actions.
 * @element umb-document-action-button
 * @fires {CustomEvent} action-click - Fired when action is clicked
 * @slot - Default slot for button content
 * @cssprop --umb-button-color - Button text color
 */
export class UmbDocumentActionButton extends LitElement {
	/**
	 * The action identifier
	 * @attr
	 * @type {string}
	 */
	@property({ type: String })
	action = '';
}
```

**TODOs**:

```typescript
// TODO: Implement pagination [NL]
// FIXME: Memory leak in subscription [JOV]
// HACK: Temporary workaround for API bug [LK]
```

**Remove Dead Code**:
- Don't comment out code, delete it (Git history preserves it)
- Remove unused imports, functions, variables
- Clean up console.logs before committing

### Patterns to Avoid

**Don't**:
- Use `var` (use `const`/`let`)
- Modify prototypes of built-in objects
- Use global variables
- Block the main thread (use web workers for heavy computation)
- Create deeply nested structures
- Use `any` type (use `unknown` or proper types)
- Use non-null assertions `!` unless absolutely necessary
- Ignore TypeScript errors with `@ts-ignore`


