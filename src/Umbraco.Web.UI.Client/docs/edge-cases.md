# Edge Cases
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---


### Null/Undefined Handling

**Always Check**:

```typescript
// Use optional chaining
const name = this._content?.name;

// Use nullish coalescing for defaults
const name = this._content?.name ?? 'Untitled';

// Check before accessing
if (!this._content) {
	return html`<p>No content</p>`;
}
```

**TypeScript Strict Null Checks** (enabled):
- Variables are non-nullable by default
- Use `Type | undefined` or `Type | null` explicitly
- TypeScript forces null checks

**Function Parameters**:

```typescript
// Make nullable parameters explicit
function load(id: string, culture?: string) {
	const cultureCode = culture ?? 'en-US'; // Default value
}
```

### Array Edge Cases

**Empty Arrays**:

```typescript
// Check length before access
if (this._items.length === 0) {
	return html`<p>No items</p>`;
}

// Safe array methods
const first = this._items[0]; // Could be undefined
const first = this._items.at(0); // Also could be undefined

// Use optional chaining
const firstId = this._items[0]?.id;
```

**Single vs Multiple Items**:

```typescript
// Handle both cases
const items = Array.isArray(data) ? data : [data];
```

**Array Methods on Undefined**:

```typescript
// Guard against undefined
const ids = this._items?.map(item => item.id) ?? [];

// Or check first
if (!this._items) {
	return [];
}
return this._items.map(item => item.id);
```

**Sparse Arrays** (rare in this codebase):

```typescript
// Use filter to remove empty slots
const dense = sparse.filter(() => true);
```

### String Edge Cases

**Empty Strings**:

```typescript
// Check for empty strings
if (!name || name.trim().length === 0) {
	return 'Untitled';
}

// Or use default
const displayName = name?.trim() || 'Untitled';
```

**String vs Null/Undefined**:

```typescript
// Distinguish between empty string and null
const hasValue = value !== null && value !== undefined;
const isEmpty = value === '';

// Or use optional chaining
const length = value?.length ?? 0;
```

**Trim Whitespace**:

```typescript
// Always trim user input
const cleanName = this._name.trim();

// Validate after trimming
if (cleanName.length === 0) {
	// Invalid
}
```

**String Encoding**:
- Use UTF-8 everywhere
- Be aware of Unicode characters (emojis, etc.)
- Use `textContent` not `innerHTML` for plain text

**Internationalization**:

```typescript
// Use localization API
const label = this.localize.term('general_submit');

// Not hardcoded strings
// const label = 'Submit'; // ❌
```

### Number Edge Cases

**NaN Checks**:

```typescript
// Use Number.isNaN, not isNaN
if (Number.isNaN(value)) {
	// Handle NaN
}

// isNaN coerces, Number.isNaN doesn't
isNaN('hello'); // true (coerces to NaN)
Number.isNaN('hello'); // false (not a number)
```

**Infinity**:

```typescript
if (!Number.isFinite(value)) {
	// Handle Infinity or NaN
}
```

**Parsing**:

```typescript
// parseInt/parseFloat can return NaN
const num = parseInt(input, 10);
if (Number.isNaN(num)) {
	// Handle invalid input
}

// Or use Number constructor with validation
const num = Number(input);
if (!Number.isFinite(num)) {
	// Invalid
}
```

**Floating Point Precision**:

```typescript
// Don't compare floats with ===
const isEqual = Math.abs(a - b) < 0.0001;

// Or use integers for currency (cents, not dollars)
const priceInCents = 1099; // $10.99
```

**Division by Zero**:

```typescript
// JavaScript returns Infinity, not error
const result = 10 / 0; // Infinity

// Check denominator
if (denominator === 0) {
	// Handle division by zero
	return 0; // Or throw error
}
```

**Safe Integer Range**:

```typescript
// JavaScript integers are safe up to Number.MAX_SAFE_INTEGER
const isSafe = Number.isSafeInteger(value);

// For IDs, use strings not numbers
interface UmbContentModel {
	id: string; // Not number
}
```

### Object Edge Cases

**Property Existence**:

```typescript
// Use optional chaining
const value = obj?.property;

// Or check explicitly
if ('property' in obj) {
	const value = obj.property;
}

// hasOwnProperty (not inherited)
if (Object.hasOwn(obj, 'property')) {
	// Property exists on object itself
}
```

**Null vs Undefined vs {}**:

```typescript
// Distinguish between missing and empty
const isEmpty = obj !== null && obj !== undefined && Object.keys(obj).length === 0;

// Or use optional chaining
const hasData = obj && Object.keys(obj).length > 0;
```

**Shallow vs Deep Copy**:

```typescript
// Shallow copy
const copy = { ...original };

// Deep copy (for simple objects)
const deepCopy = JSON.parse(JSON.stringify(original));

// Deep copy with structuredClone (modern browsers)
const deepCopy = structuredClone(original);

// Note: Functions, symbols, and undefined are not copied by JSON.stringify
```

**Object Freezing**:

```typescript
// Prevent modification
const frozen = Object.freeze(obj);

// Check if frozen
if (Object.isFrozen(obj)) {
	// Can't modify
}
```

### Async/Await Edge Cases

**Unhandled Promise Rejections**:

```typescript
// Always catch errors
try {
	await asyncOperation();
} catch (error) {
	console.error('Failed:', error);
}

// Or use .catch()
asyncOperation().catch(error => {
	console.error('Failed:', error);
});

// For fire-and-forget, explicitly catch
void asyncOperation().catch(error => console.error(error));
```

**Promise.all Fails Fast**:

```typescript
// Promise.all rejects if ANY promise rejects
try {
	const results = await Promise.all([op1(), op2(), op3()]);
} catch (error) {
	// One failed, others may still be running
}

// Use Promise.allSettled to wait for all (even if some fail)
const results = await Promise.allSettled([op1(), op2(), op3()]);
results.forEach((result, index) => {
	if (result.status === 'fulfilled') {
		console.log(`Op ${index} succeeded:`, result.value);
	} else {
		console.error(`Op ${index} failed:`, result.reason);
	}
});
```

**Race Conditions**:

```typescript
// Avoid race conditions with sequential operations
this._loading = true;
try {
	const data1 = await fetchData1();
	const data2 = await fetchData2(data1.id);
	this._result = processData(data1, data2);
} finally {
	this._loading = false;
}

// For parallel operations, use Promise.all
const [data1, data2] = await Promise.all([fetchData1(), fetchData2()]);
```

**Timeout Handling**:

```typescript
// Implement timeout for operations
function withTimeout<T>(promise: Promise<T>, ms: number): Promise<T> {
	return Promise.race([
		promise,
		new Promise<T>((_, reject) =>
			setTimeout(() => reject(new Error('Timeout')), ms)
		),
	]);
}

// Usage
try {
	const data = await withTimeout(fetchData(), 5000);
} catch (error) {
	// Handle timeout or other errors
}
```

**Memory Leaks with Event Listeners**:

```typescript
export class UmbMyElement extends LitElement {
	#controller = new UmbMyController(this);

	// Controllers automatically clean up on disconnect
	constructor() {
		super();
		this.#controller.observe(dataSource$, (value) => {
			this._data = value;
		});
	}

	// Lit lifecycle handles this automatically
	disconnectedCallback() {
		super.disconnectedCallback();
		// Controllers are destroyed automatically
	}
}
```

### Web Component Edge Cases

**Custom Element Not Defined**:

```typescript
// Check if element is defined
if (!customElements.get('umb-my-element')) {
	// Not defined yet
	await customElements.whenDefined('umb-my-element');
}

// Or use upgrade
await customElements.upgrade(element);
```

**Shadow DOM Queries**:

```typescript
// Query shadow root, not document
const button = this.shadowRoot?.querySelector('button');

// Use Lit decorators
@query('#myButton')
private _button!: HTMLButtonElement;
```

**Property vs Attribute Sync**:

```typescript
// Lit keeps properties and attributes in sync
@property({ type: String })
name = ''; // Syncs with name="" attribute

// But complex types don't sync to attributes
@property({ type: Object })
data = {}; // No attribute sync

// State doesn't sync to attributes
@state()
private _loading = false;
```

**Reactive Update Timing**:

```typescript
// Wait for update to complete
this.value = 'new value';
await this.updateComplete;
// Now DOM is updated

// Or use requestUpdate
this.requestUpdate();
await this.updateComplete;
```

### Date/Time Edge Cases

**Use Luxon** (not Date):

```typescript
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';

// Create dates
const now = DateTime.now();
const utc = DateTime.utc();
const parsed = DateTime.fromISO('2024-01-15T10:30:00Z');

// Always store dates in UTC
const isoString = now.toUTC().toISO();

// Format for display
const formatted = now.toLocaleString(DateTime.DATETIME_MED);

// Timezone handling
const local = utc.setZone('local');
```

**Date Comparison**:

```typescript
// Compare DateTime objects
if (date1 < date2) { }
if (date1.equals(date2)) { }

// Or compare timestamps
if (date1.toMillis() < date2.toMillis()) { }
```

**Date Parsing Can Fail**:

```typescript
const date = DateTime.fromISO(input);
if (!date.isValid) {
	console.error('Invalid date:', date.invalidReason);
	// Handle invalid date
}
```

### JSON Edge Cases

**JSON.parse Can Throw**:

```typescript
// Always wrap in try/catch
try {
	const data = JSON.parse(jsonString);
} catch (error) {
	console.error('Invalid JSON:', error);
	// Handle parse error
}
```

**Circular References**:

```typescript
// JSON.stringify throws on circular references
const obj = { a: 1 };
obj.self = obj;

try {
	JSON.stringify(obj);
} catch (error) {
	// TypeError: Converting circular structure to JSON
}
```

**Date Objects**:

```typescript
// Dates become strings
const data = { date: new Date() };
const json = JSON.stringify(data);
const parsed = JSON.parse(json);
// parsed.date is a string, not Date

// Use ISO format explicitly
const isoDate = new Date().toISOString();
```

**Undefined Values**:

```typescript
// Undefined values are omitted
const obj = { a: 1, b: undefined };
JSON.stringify(obj); // '{"a":1}'

// Use null for explicit absence
const obj = { a: 1, b: null };
JSON.stringify(obj); // '{"a":1,"b":null}'
```

### Handling Strategy

**Guard Clauses**:

```typescript
// Check preconditions early
if (!this._data) return;
if (this._data.length === 0) return;
if (!this._data[0].name) return;

// Now can safely use data
this.processData(this._data[0].name);
```

**Defensive Programming**:

```typescript
// Validate inputs
function process(items: unknown) {
	if (!Array.isArray(items)) {
		throw new Error('Expected array');
	}
	// Safe to use items as array
}

// Use type guards
if (isContentModel(data)) {
	// TypeScript knows data is UmbContentModel
}
```

**Fail Fast**:

```typescript
// Throw errors early for programmer mistakes
if (this._repository === undefined) {
	throw new Error('Repository not initialized');
}

// Handle expected errors gracefully
try {
	const data = await this._repository.loadById(id);
} catch (error) {
	this._error = 'Failed to load content';
	return;
}
```

**Document Edge Cases**:

```typescript
/**
 * Loads content by ID.
 * @throws {UmbContentNotFoundError} If content doesn't exist
 * @throws {UmbUnauthorizedError} If user lacks permission
 * @returns {Promise<UmbContentModel>} The content model
 *
 * @remarks
 * This method returns cached data if available.
 * Pass `{ skipCache: true }` to force a fresh load.
 */
async loadById(id: string, options?: LoadOptions): Promise<UmbContentModel> {
	// ...
}
```


