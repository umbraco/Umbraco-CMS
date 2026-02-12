# Style Guide
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---


### Naming Conventions

**Files**:
- Web components: `my-component.element.ts`
- Tests: `my-component.test.ts`
- Stories: `my-component.stories.ts`
- Controllers: `my-component.controller.ts`
- Contexts: `my-component.context.ts`
- Modals: `my-component.modal.ts`
- Workspaces: `my-component.workspace.ts`
- Repositories: `my-component.repository.ts`
- Index files: `index.ts` (barrel exports)

**Classes & Types**:
- Classes: `PascalCase` with `Umb` prefix: `UmbMyComponent`
- Interfaces: `PascalCase` with `Umb` prefix: `UmbMyInterface`
- Types: `PascalCase` with `Umb`, `Ufm`, `Manifest`, `Meta`, or `Example` prefix
- Exported types MUST have approved prefix
- Example types for docs: `ExampleMyType`

**Variables & Functions**:
- Public members: `camelCase` without underscore: `myVariable`, `myMethod`
- Private members: `camelCase` with leading underscore: `_myPrivateVariable`
- #private members: `camelCase` without underscore: `#myPrivateField`
- Protected members: `camelCase` with optional underscore: `myProtected` or `_myProtected`
- Constants (exported): `UPPER_SNAKE_CASE` with `UMB_` prefix: `UMB_MY_CONSTANT`
- Local constants: `UPPER_CASE` or `camelCase`

**Custom Elements**:
- Element tag names: kebab-case with `umb-` prefix: `umb-my-component`
- Must be registered in global `HTMLElementTagNameMap`

### File Organization

- One class/component per file
- Use barrel exports (`index.ts`) for package public APIs
- Import order (enforced by ESLint):
  1. External dependencies
  2. Parent imports
  3. Sibling imports
  4. Index imports
  5. Type-only imports (separate)

### Code Formatting (Prettier)

```json
{
  "printWidth": 120,
  "singleQuote": true,
  "semi": true,
  "bracketSpacing": true,
  "bracketSameLine": true,
  "useTabs": true
}
```

- **Indentation**: Tabs (not spaces)
- **Line length**: 120 characters max
- **Quotes**: Single quotes
- **Semicolons**: Required
- **Trailing commas**: Yes (default)

### TypeScript Conventions

**Strict Mode** (enabled in `tsconfig.json`):
- `strict: true`
- `noImplicitReturns: true`
- `noFallthroughCasesInSwitch: true`
- `noImplicitOverride: true`

**Type Features**:
- Use TypeScript types over JSDoc when possible
- BUT: Lit components use JSDoc for web-component-analyzer compatibility
- Use `type` for unions/intersections: `type MyType = A | B`
- Use `interface` for object shapes and extension: `interface MyInterface extends Base`
- Prefer `const` over `let`, never use `var`
- Use `readonly` for immutable properties
- Use generics for reusable code
- Avoid `any` (lint warning), use `unknown` instead
- Use type guards for narrowing
- Use `as const` for literal types

**Module Syntax**:
- ES Modules only: `import`/`export`
- Use consistent type imports: `import type { MyType } from '...'`
- Use consistent type exports: `export type { MyType }`
- No side-effects in imports

**Decorators**:
- `@customElement('umb-my-element')` - Register custom element
- `@property({ type: String })` - Reactive properties
- `@state()` - Internal reactive state
- `@query('#myId')` - Query shadow DOM
- Experimental decorators enabled

### Modern TypeScript Features to Use

- **Async/await** over callbacks
- **Optional chaining**: `obj?.property?.method?.()`
- **Nullish coalescing**: `value ?? defaultValue`
- **Template literals**: `` `Hello ${name}` ``
- **Destructuring**: `const { a, b } = obj`
- **Spread operator**: `{ ...obj, newProp: value }`
- **Arrow functions**: `const fn = () => {}`
- **Array methods**: `map`, `filter`, `reduce`, `find`, `some`, `every`
- **Object methods**: `Object.keys`, `Object.values`, `Object.entries`
- **Private fields**: `#privateField`

### Event Handler Guidelines

**Event handlers must be arrow function properties** to prevent memory leaks:

```typescript
// ✅ GOOD: Arrow function property
export class UmbMyElement extends LitElement {
	#onStorageEvent = async (evt: StorageEvent) => {
		// Handler logic
	};

	constructor() {
		super();
		window.addEventListener('storage', this.#onStorageEvent);
	}

	disconnectedCallback() {
		window.removeEventListener('storage', this.#onStorageEvent);
	}
}

// ❌ BAD: Using .bind(this) creates new function references
export class UmbBadElement extends LitElement {
	constructor() {
		super();
		// Each .bind(this) creates a NEW reference!
		window.addEventListener('storage', this.#onStorageEvent.bind(this));
	}

	disconnectedCallback() {
		// This creates ANOTHER reference - doesn't remove the original!
		window.removeEventListener('storage', this.#onStorageEvent.bind(this));
	}

	#onStorageEvent(evt: StorageEvent) {
		// Handler logic
	}
}
```

**Placement in Class**:
- Place event handler arrow functions near the top of the class with other properties
- Place them after state properties but before constructor
- Add a comment indicating they are event handlers

**Naming**:
- Private handlers: `#onEventName` or `#handleEventName`
- Protected handlers: `_onEventName` or `_handleEventName`
- Use descriptive names: `#onStorageEvent`, `#handleDragEnter`, `#onActionExecuted`

### Language Features to Avoid

- `var` (use `const`/`let`)
- `eval()` or `Function()` constructor
- `with` statement
- `arguments` object (use rest parameters)
- Deeply nested callbacks (use async/await)
- Non-null assertions unless absolutely necessary: `value!`
- Type assertions unless necessary: `value as Type`
- `@ts-ignore` (use `@ts-expect-error` with comment)

### Custom ESLint Rules

**Project-Specific Rules**:
- `prefer-static-styles-last` - Static styles property must be last in class
- `enforce-umbraco-external-imports` - External dependencies must be imported via `@umbraco-cms/backoffice/external/*`
- Private members MUST have leading underscore
- Exported types MUST have approved prefix (Umb, Ufm, Manifest, Meta, Example)
- Exported string constants MUST have UMB_ prefix
- Semicolons required
- No `var` keyword
- No circular dependencies (max depth 6)
- No self-imports
- Consistent type imports/exports

**Allowed JSDoc Tags** (for web-component-analyzer):
- `@element` - Element name
- `@attr` - HTML attribute
- `@fires` - Custom events
- `@prop` - Properties
- `@slot` - Slots
- `@cssprop` - CSS custom properties
- `@csspart` - CSS parts

### Documentation

- **Public APIs**: JSDoc comments with `@description`, `@param`, `@returns`, `@example`
- **Web Components**: JSDoc with web-component-analyzer tags
- **Complex logic**: Inline comments explaining "why" not "what"
- **TODOs**: Format as `// TODO: description [initials]`
- **Deprecated**: Use `@deprecated` tag with migration instructions


