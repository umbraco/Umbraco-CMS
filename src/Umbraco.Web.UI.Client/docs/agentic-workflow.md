# Agentic Workflow
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---

### Phase 1: Analysis & Planning

**Understand Requirements**:
1. Read user request carefully
2. Identify acceptance criteria
3. Ask clarifying questions if ambiguous
4. Determine scope (new feature, bug fix, refactor, etc.)

**Research Codebase**:
1. Find similar implementations in codebase
2. Identify patterns and conventions used
3. Locate relevant packages and modules
4. Review existing tests for similar features
5. Check for existing utilities or helpers

**Identify Technical Approach**:
1. Which packages need changes?
   - New package or modify existing?
   - Core infrastructure or feature package?
2. What patterns to use?
   - Web Component, Controller, Repository, Context?
   - Extension type? (dashboard, workspace, modal, etc.)
3. Dependencies needed?
   - New npm packages?
   - Internal package dependencies?
4. API changes needed?
   - New OpenAPI endpoints?
   - Changes to existing endpoints?

**Break Down Implementation**:
1. **Models/Types** - Define TypeScript interfaces and types
2. **API Client** - Update OpenAPI client if backend changes
3. **Repository** - Data access layer
4. **Store/Context** - State management
5. **Controller** - Business logic
6. **Element** - UI component
7. **Manifest** - Extension registration
8. **Tests** - Unit and integration tests
9. **Documentation** - JSDoc and examples
10. **Storybook** (if applicable) - Component stories

**Consider Architecture**:
- Does this follow project patterns?
- Are dependencies correct? (libs → packages → apps)
- Will this create circular dependencies?
- Is this extensible for future needs?
- Performance implications?

**Document Plan**:
- Write brief implementation plan
- Identify potential issues or blockers
- Get approval from user if significant changes

### Phase 2: Incremental Implementation

**For New Feature (Component/Package)**:

**Step 1: Define Types**
```typescript
// 1. Create model interface
export interface UmbMyModel {
	id: string;
	name: string;
	description?: string;
}

// 2. Create manifest type
export interface UmbMyManifest extends UmbManifestBase {
	type: 'myType';
	// ... specific properties
}
```

**Verify**: TypeScript compiles, no errors

**Step 2: Create Repository**
```typescript
// 3. Data access layer
export class UmbMyRepository {
	async requestById(id: string) {
		// Fetch from API
		// Return { data } or { error }
	}
}
```

**Verify**: Repository compiles, basic structure correct

**Step 3: Create Store/Context**
```typescript
// 4. State management
export class UmbMyStore extends UmbStoreBase {
	// Observable state
}

export const UMB_MY_CONTEXT = new UmbContextToken<UmbMyContext>('UmbMyContext');

export class UmbMyContext extends UmbControllerBase {
	#repository = new UmbMyRepository();
	#store = new UmbMyStore(this);

	// Public API
}
```

**Verify**: Context compiles, can be consumed

**Step 4: Create Element**
```typescript
// 5. UI component
@customElement('umb-my-element')
export class UmbMyElement extends UmbLitElement {
	#context?: typeof UMB_MY_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_MY_CONTEXT, (context) => {
			this.#context = context;
		});
	}

	render() {
		return html`<div>My Element</div>`;
	}
}
```

**Verify**: Element renders in browser, no console errors

**Step 5: Wire Up Interactions**
```typescript
// 6. Connect user interactions to logic
@customElement('umb-my-element')
export class UmbMyElement extends UmbLitElement {
    @state()
    private _data?: UmbMyModel;

	async #handleClick() {
		const { data, error } = await this.#context?.load();
		if (error) {
			this._error = error;
			return;
		}
		this._data = data;
	}

	render() {
		return html`
			<uui-button @click=${this.#handleClick} label="Load"></uui-button>
			${this._data ? html`<p>${this._data.name}</p>` : nothing}
		`;
	}
}
```

**Verify**: Interactions work, data flows correctly

**Step 6: Add Extension Manifest**
```typescript
// 7. Register extension
export const manifest: UmbManifestMyType = {
	type: 'myType',
	alias: 'My.Extension',
	name: 'My Extension',
	element: () => import('./my-element.element.js'),
};
```

**Verify**: Extension loads, manifest is valid

**Step 7: Write Tests**
```typescript
// 8. Unit tests
describe('UmbMyElement', () => {
	it('should render', async () => {
		const element = await fixture(html`<umb-my-element></umb-my-element>`);
		expect(element).to.exist;
	});

	it('should load data', async () => {
		// Test data loading
	});
});
```

**Verify**: Tests pass

**Step 8: Add Error Handling**
```typescript
// 9. Handle errors gracefully
async #handleClick() {
	try {
		this._loading = true;
		const { data, error } = await this.#context?.load();
		if (error) {
			this._error = 'Failed to load data';
			return;
		}
		this._data = data;
	} catch (error) {
		this._error = 'Unexpected error occurred';
		console.error('Load failed:', error);
	} finally {
		this._loading = false;
	}
}
```

**Step 9: Add localization**
```typescript
// 10. Add to src/Umbraco.Web.UI.Client/src/assets/lang/en.ts and other appropriate files
{
  actions: {
    load: 'Load'
  }
}

// 11. Use the localize helper (`this.localize.term()`) in the element
render() {
  return html`
	<uui-button @click=${this.#handleClick} label=${this.localize.term('actions_load')></uui-button>
		${this._data ? html`<p>${this._data.name}</p>` : ''}
	`;
}

async #handleClick() {
	try {
		this._loading = true;
		const { data, error } = await this.#context?.load();
		if (error) {
			this._error = this.localize.term('errors_receivedErrorFromServer');
			return;
		}
		this._data = data;
	} catch (error) {
		this._error = this.localize.term('errors_defaultError');
		console.error('Load failed:', error);
	} finally {
		this._loading = false;
	}
}

// 12. Outside elements (such as controllers), use the Localization Controller
export class UmbMyController extends UmbControllerBase {
  #localize = new UmbLocalizationController(this);
  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

  constructor(host: UmbControllerHost) {
    super(host);

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });
  }

  fetchData() {
    // Show error
    this.#notificationContext?.peek('positive', {
      data: {
        headline: this.#localize.term('speechBubbles_onlineHeadline'),
        message: this.#localize.term('speechBubbles_onlineMessage'),
      },
    });
  }
}   
**Verify**: Errors are handled, UI shows error state

**After Each Step**:
- ✅ Code compiles (no TypeScript errors)
- ✅ Tests pass (existing and new)
- ✅ Follows patterns (consistent with codebase)
- ✅ No breaking changes (or documented)
- ✅ ESLint passes
- ✅ Commit working code

### Phase 3: Review & Refinement

**Run All Quality Checks**:

```bash
# 1. Run all tests
npm test

# 2. Run lint
npm run lint:errors

# 3. Type check
npm run compile

# 4. Build
npm run build

# 5. Check circular dependencies
npm run check:circular

# 6. Validate package exports
npm run package:validate
```

**Code Review Checklist**:
- [ ] Code follows style guide
- [ ] All tests pass
- [ ] New tests added for new code
- [ ] TypeScript types are correct
- [ ] No `any` types (or justified)
- [ ] JSDoc comments on public APIs
- [ ] Error handling in place
- [ ] Edge cases handled
- [ ] No console.log in production code
- [ ] Accessibility considered (for UI components)
- [ ] Performance acceptable
- [ ] No memory leaks (subscriptions cleaned up)
- [ ] Follows existing patterns
- [ ] No circular dependencies introduced

**Documentation**:
- [ ] JSDoc on public APIs
- [ ] Update README if needed
- [ ] Add Storybook story (for components)
- [ ] Add example code (for APIs)
- [ ] Update CLAUDE.md (for significant changes)

### Reasoning & Decision-Making

**Architecture Decisions**:
- **Follow existing patterns** - Don't invent new patterns unless necessary
- **Use Context API** - For cross-component state and dependencies
- **Controllers for logic** - Separate business logic from UI
- **Repositories for data** - Abstract data access
- **Manifests for extensions** - Register features via extension system

**Component Design Decisions**:
- **Small components** - Break complex UI into smaller components
- **Single responsibility** - Each component does one thing well
- **Composition** - Combine small components to build complex UI
- **Props down, events up** - Data flows down via properties, changes flow up via events

**State Management Decisions**:
- **Local state** - `@state()` for component-specific state
- **Shared state** - Context and stores for cross-component state
- **Observables** - RxJS for reactive data streams
- **Immutability** - Don't mutate state, create new objects

**API Design Decisions**:
- **Async by default** - All data operations are async
- **Error objects** - Return `{ data, error }` not throw exceptions
- **TypeScript types** - Strong typing for all APIs
- **Consistent naming** - Follow existing naming conventions

### Think-Aloud Process

**Example: "I need to add a bulk action for content"**

**Analysis**:
> "I need to add a bulk action for content. Let me think through this:
>
> 1. What is a bulk action? It's an action that operates on multiple items.
> 2. Where do bulk actions appear? In collection views (document tree, list view).
> 3. What pattern is used? Let me check existing bulk actions...
> 4. Found: Block editor has bulk actions. Let me review that code.
> 5. Architecture: Bulk actions are registered via manifests (type: 'entityBulkAction')
> 6. Implementation needs:
>    - Manifest to register action
>    - Element to show confirmation dialog
>    - Repository method to perform bulk operation
>    - Permission check (can user do this?)
> 7. Which package? This goes in `packages/documents/documents/` since it's document-specific.
> 8. Dependencies: Need `@umbraco-cms/backoffice/entity-bulk-action`, modal, repository."

**Planning**:
> "Implementation steps:
> 1. Create bulk action manifest in documents package
> 2. Create modal element for confirmation
> 3. Add repository method for bulk operation
> 4. Add permission check
> 5. Wire up action to modal
> 6. Test with multiple documents
> 7. Handle errors (partial success, permissions, etc.)
> 8. Add loading state
> 9. Show success/error notification
> 10. Write tests"

**Implementation**:
> "Starting with step 1: Create manifest.
> Looking at existing bulk actions, I see the pattern uses `UmbEntityBulkActionBase`.
> Let me create the manifest following that pattern..."

### Error Recovery

**If Tests Fail**:
1. Read the error message carefully
2. Reproduce the failure locally
3. Debug with console.log or debugger
4. Understand root cause (don't just fix symptoms)
5. Fix the underlying issue
6. Verify fix doesn't break other tests
7. Add test for the bug (if it was a real bug)

**If Architecture Feels Wrong**:
1. Pause and reconsider
2. Review similar implementations in codebase
3. Discuss with team (via PR comments)
4. Don't force a pattern that doesn't fit
5. Refactor if needed

**If Introducing Breaking Changes**:
1. Discuss with user first
2. Document the breaking change
3. Provide migration path
4. Update CHANGELOG
5. Consider deprecation instead

**If Stuck**:
1. Ask for clarification from user
2. Review documentation
3. Look at similar code in repository
4. Break problem into smaller pieces
5. Try a simpler approach first

### Quality Gates Checklist

Before marking work as complete:

**Code Quality**:
- [ ] All new code has unit tests
- [ ] Integration tests for workflows (if applicable)
- [ ] All tests pass (including existing tests)
- [ ] Code follows style guide (ESLint passes)
- [ ] Prettier formatting correct
- [ ] No TypeScript errors
- [ ] JSDoc comments on public functions
- [ ] No `console.log` in production code
- [ ] No commented-out code

**Security**:
- [ ] Input validation in place
- [ ] No XSS vulnerabilities
- [ ] No sensitive data logged
- [ ] User input sanitized
- [ ] Permissions checked (for sensitive operations)

**Performance**:
- [ ] No obvious performance issues
- [ ] No memory leaks (subscriptions cleaned up)
- [ ] Efficient algorithms used
- [ ] Large lists use virtualization (if needed)

**Architecture**:
- [ ] Follows existing patterns
- [ ] No circular dependencies
- [ ] Correct package dependencies
- [ ] Extension manifest correct (if applicable)

**Documentation**:
- [ ] JSDoc on public APIs
- [ ] README updated (if needed)
- [ ] Examples added (if new API)
- [ ] Breaking changes documented

**User Experience** (for UI changes):
- [ ] Accessible (keyboard navigation, screen readers)
- [ ] Responsive (works on different screen sizes)
- [ ] Loading states shown
- [ ] Errors handled gracefully
- [ ] Success feedback provided

### Communication

**After Each Phase**:
- Summarize what was implemented
- Highlight key decisions and why
- Call out any blockers or questions
- Show progress (code snippets, screenshots for UI)

**When Making Decisions**:
- Explain reasoning
- Reference existing patterns
- Call out trade-offs
- Ask for confirmation on significant changes

**When Blocked**:
- Clearly state the blocker
- Explain what you've tried
- Ask specific questions
- Suggest potential approaches

**When Complete**:
- Summarize what was delivered
- Note any deviations from plan (and why)
- Highlight testing performed
- Mention any follow-up work needed

### For Large Features

**Multi-Session Features**:
1. Create feature branch (if spanning multiple PRs)
2. Break into multiple PRs if very large:
   - PR 1: Infrastructure (types, repository, context)
   - PR 2: UI components
   - PR 3: Extensions and integration
3. Keep main stable - only merge completed, tested features
4. Consider feature flags for gradual rollout
5. Document progress and remaining work in PR description

**Coordination**:
- Update PR description with progress
- Use GitHub task lists in PR description
- Comment on PR with status updates
- Request early feedback on architecture

---

