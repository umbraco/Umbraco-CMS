# Testing
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---


### Testing Philosophy

- **Unit tests** for business logic and utilities
- **Component tests** for web components
- **Integration tests** for workflows
- **E2E tests** for critical user journeys
- **Coverage target**: No strict requirement, focus on meaningful tests
- **Test pyramid**: Many unit tests, fewer integration tests, few E2E tests

### Testing Frameworks

**Unit/Component Testing**:
- `@web/test-runner` - Fast test runner for web components
- `@open-wc/testing` - Testing utilities, includes Chai assertions
- `@types/chai` - Assertion library
- `@types/mocha` - Test framework (used by @web/test-runner)
- `@web/test-runner-playwright` - Browser launcher
- `element-internals-polyfill` - Polyfill for form-associated custom elements

**E2E Testing**:
- `@playwright/test` - End-to-end testing in real browsers
- Playwright MSW integration for API mocking

**Test Utilities**:
- `@umbraco-cms/internal/test-utils` - Shared test utilities
- MSW (Mock Service Worker) - API mocking
- Fixtures in `src/mocks/data/` - Test data

### Test Project Organization

```
src/
├── **/*.test.ts              # Unit tests co-located with source
├── mocks/
│   ├── data/                 # Mock data & in-memory databases
│   └── handlers/             # MSW request handlers
├── examples/
│   └── **/*.test.ts          # Example tests
└── utils/
    └── test-utils.ts         # Shared test utilities

e2e/
├── **/*.spec.ts              # Playwright E2E tests
└── fixtures/                 # E2E test fixtures
```

### Test Naming Convention

```typescript
describe('UmbMyComponent', () => {
	describe('initialization', () => {
		it('should create element', async () => {
			// test
		});

		it('should set default properties', async () => {
			// test
		});
	});

	describe('user interactions', () => {
		it('should emit event when button clicked', async () => {
			// test
		});
	});
});
```

### Test Structure (AAA Pattern)

```typescript
it('should do something', async () => {
	// Arrange - Set up test data and conditions
	const element = await fixture<UmbMyElement>(html`<umb-my-element></umb-my-element>`);
	const spy = sinon.spy();
	element.addEventListener('change', spy);

	// Act - Perform the action
	element.value = 'new value';
	await element.updateComplete;

	// Assert - Verify the results
	expect(spy.calledOnce).to.be.true;
	expect(element.value).to.equal('new value');
});
```

### Unit Test Guidelines

**What to Test**:
- Public API methods and properties
- User interactions (clicks, inputs, etc.)
- State changes
- Event emissions
- Error handling
- Edge cases

**What NOT to Test**:
- Private implementation details
- Framework/library code
- Generated code (e.g., OpenAPI clients)
- Third-party dependencies

**Best Practices**:
- One assertion per test (when practical)
- Test behavior, not implementation
- Use meaningful test names (describe what should happen)
- Keep tests fast (<100ms each)
- Isolate tests (no shared state between tests)
- Use fixtures for DOM elements
- Use spies/stubs for external dependencies
- Clean up after tests (auto-handled by @web/test-runner)

**Web Component Testing**:

```typescript
import { fixture, html, expect } from '@open-wc/testing';
import { UmbMyElement } from './my-element.element';

describe('UmbMyElement', () => {
	let element: UmbMyElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-my-element></umb-my-element>`);
	});

	it('should render', () => {
		expect(element).to.exist;
		expect(element.shadowRoot).to.exist;
	});

	it('should be accessible', async () => {
		await expect(element).to.be.accessible();
	});
});
```

### Integration Test Guidelines

Integration tests verify interactions between multiple components/systems:

```typescript
describe('UmbContentRepository', () => {
	let repository: UmbContentRepository;
	let mockContext: UmbMockContext;

	beforeEach(() => {
		mockContext = new UmbMockContext();
		repository = new UmbContentRepository(mockContext);
	});

	it('should fetch and cache content', async () => {
		const result = await repository.requestById('content-123');
		expect(result.data).to.exist;
		// Verify caching behavior
		const cached = await repository.requestById('content-123');
		expect(cached.data).to.equal(result.data);
	});
});
```

### E2E Test Guidelines

E2E tests with Playwright:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Content Editor', () => {
	test('should create new document', async ({ page }) => {
		await page.goto('/');
		await page.click('[data-test="create-document"]');
		await page.fill('[data-test="document-name"]', 'My New Page');
		await page.click('[data-test="save"]');

		await expect(page.locator('[data-test="success-notification"]')).toBeVisible();
	});
});
```

### Mocking Best Practices

**MSW (Mock Service Worker)** for API mocking:

```typescript
import { rest } from 'msw';

export const handlers = [
	rest.get('/umbraco/management/api/v1/document/:id', (req, res, ctx) => {
		const { id } = req.params;
		return res(
			ctx.json({
				id,
				name: 'Test Document',
				// ... mock data
			})
		);
	}),
];
```

**Context Mocking**:

```typescript
import { UmbMockContext } from '@umbraco-cms/internal/test-utils';

const mockContext = new UmbMockContext();
mockContext.provideContext(UMB_AUTH_CONTEXT, mockAuthContext);
```

### Running Tests

**Local Development**:

```bash
# Run all tests once
npm test

# Run in watch mode
npm run test:watch

# Run with dev config (faster, less strict)
npm run test:dev

# Run in watch mode with dev config
npm run test:dev-watch

# Run specific test file pattern
npm test -- --files "**/my-component.test.ts"
```

**E2E Tests**:

```bash
# Run E2E tests
npm run test:e2e

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific test
npx playwright test e2e/content-editor.spec.ts

# Debug mode
npx playwright test --debug
```

**CI/CD**:
- All tests run on pull requests
- E2E tests run on Chromium in CI
- Retries: 2 attempts on CI, 0 locally
- Parallel: Sequential in CI, parallel locally

### Coverage

Coverage reporting is currently disabled (see `web-test-runner.config.mjs`):

```javascript
/* TODO: fix coverage report
coverageConfig: {
	reporters: ['lcovonly', 'text-summary'],
},
*/
```

**What to Exclude from Coverage**:
- Test files themselves
- Mock data and handlers
- Generated code (OpenAPI clients, icons)
- External wrapper modules
- Type declaration files


