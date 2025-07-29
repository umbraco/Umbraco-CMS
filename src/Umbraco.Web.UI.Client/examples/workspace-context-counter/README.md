# Workspace Extensions Complete Example

This comprehensive example demonstrates all 5 workspace extension types working together as an integrated system. It showcases how workspace extensions can communicate through a shared workspace context to create cohesive functionality.

{% hint style="info" %}
This example provides a complete working implementation that you can use as a reference when building your own workspace extensions.
{% endhint %}

## Overview

The Workspace Context serves as the central communication hub for all workspace extensions. In this example, the context manages a counter that can be manipulated and displayed by different extension types, demonstrating the power of shared state management in workspace extensions.

## Extension Types Demonstrated

### Workspace Context

The foundation of the system - provides shared state management.

{% code title="counter-workspace-context.ts" %}
```typescript
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

export class WorkspaceContextCounterElement extends UmbContextBase {
	#counter = new UmbNumberState(0);
	readonly counter = this.#counter.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, EXAMPLE_COUNTER_CONTEXT);
	}

	increment() {
		this.#counter.setValue(this.#counter.value + 1);
	}

	reset() {
		this.#counter.setValue(0);
	}
}

export const api = WorkspaceContextCounterElement;

export const EXAMPLE_COUNTER_CONTEXT = new UmbContextToken<WorkspaceContextCounterElement>(
	'UmbWorkspaceContext',
	'example.workspaceContext.counter',
);
```
{% endcode %}

**Key Features:**
- Uses `UmbNumberState` for reactive state management
- Exposes methods for state manipulation (`increment()`, `reset()`)
- Provides observable state for other extensions to consume

### Workspace Action

Primary action button that appears in the workspace footer.

{% code title="incrementor-workspace-action.ts" %}
```typescript
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { UmbWorkspaceActionBase, type UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export class ExampleIncrementorWorkspaceAction extends UmbWorkspaceActionBase implements UmbWorkspaceAction {
	override async execute() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		if (!context) {
			throw new Error('Could not get the counter context');
		}
		context.increment();
	}
}

export const api = ExampleIncrementorWorkspaceAction;
```
{% endcode %}

**Key Features:**
- Extends `UmbWorkspaceActionBase` for workspace integration
- Consumes the workspace context to perform actions
- Provides primary user interaction point

### Workspace Action Menu Item

Dropdown menu item that extends workspace actions with additional functionality.

{% code title="reset-counter-menu-item.action.ts" %}
```typescript
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { UmbWorkspaceActionMenuItemBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/workspace';

export class ExampleResetCounterMenuItemAction extends UmbWorkspaceActionMenuItemBase implements UmbWorkspaceActionMenuItem {
	override async execute() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		if (!context) {
			throw new Error('Could not get the counter context');
		}
		context.reset();
	}
}

export const api = ExampleResetCounterMenuItemAction;
```
{% endcode %}

**Key Features:**
- Extends workspace actions with additional menu options
- Uses `forWorkspaceActions` manifest property to associate with specific actions
- Provides secondary functionality (reset vs increment)

### Workspace View

Tab-based content display that shows the current state.

{% code title="counter-workspace-view.ts" %}
```typescript
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-counter-workspace-view')
export class ExampleCounterWorkspaceView extends UmbElementMixin(LitElement) {
	#counterContext?: typeof EXAMPLE_COUNTER_CONTEXT.TYPE;

	@state()
	private count = 0;

	constructor() {
		super();
		this.consumeContext(EXAMPLE_COUNTER_CONTEXT, (instance) => {
			this.#counterContext = instance;
			this.#observeCounter();
		});
	}

	#observeCounter(): void {
		if (!this.#counterContext) return;
		this.observe(this.#counterContext.counter, (count) => {
			this.count = count;
		});
	}

	override render() {
		return html`
			<uui-box class="uui-text">
				<h1 class="uui-h2">Counter Example</h1>
				<p class="uui-lead">Current count value: ${this.count}</p>
				<p>This workspace view consumes the Counter Context and displays the current count.</p>
			</uui-box>
		`;
	}
}
```
{% endcode %}

**Key Features:**
- Uses `consumeContext()` to access workspace context
- Observes state changes for reactive updates
- Provides dedicated UI for viewing/interacting with workspace data

### Workspace Footer App

Status indicator that provides persistent information display.

{% code title="counter-status-footer-app.element.ts" %}
```typescript
import { customElement, html, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';

@customElement('example-counter-status-footer-app')
export class ExampleCounterStatusFooterAppElement extends UmbElementMixin(LitElement) {
	@state()
	private _counter = 0;

	constructor() {
		super();
		this.#observeCounter();
	}

	async #observeCounter() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		if (!context) return;
		
		this.observe(
			context.counter,
			(counter: number) => {
				this._counter = counter;
			},
		);
	}

	override render() {
		return html`<span>Counter: ${this._counter}</span>`;
	}
}
```
{% endcode %}

**Key Features:**
- Provides persistent status information in workspace footer
- Uses lightweight rendering for performance
- Shows contextual information without taking up main workspace area

## Manifest Configuration

The complete system is registered through a single manifest file:

{% code title="index.ts" %}
```typescript
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	// Workspace Context
	{
		type: 'workspaceContext',
		name: 'Example Counter Workspace Context',
		alias: 'example.workspaceCounter.counter',
		api: () => import('./counter-workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	// Workspace Action
	{
		type: 'workspaceAction',
		kind: 'default',
		name: 'Example Count Incrementor Workspace Action',
		alias: 'example.workspaceAction.incrementor',
		weight: 1000,
		api: () => import('./incrementor-workspace-action.js'),
		meta: {
			label: 'Increment',
			look: 'primary',
			color: 'danger',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	// Workspace Action Menu Item
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'example.workspaceActionMenuItem.resetCounter',
		name: 'Reset Counter Menu Item',
		api: () => import('./reset-counter-menu-item.action.js'),
		forWorkspaceActions: 'example.workspaceAction.incrementor',
		weight: 100,
		meta: {
			label: 'Reset Counter',
			icon: 'icon-refresh',
		},
	},
	// Workspace View
	{
		type: 'workspaceView',
		name: 'Example Counter Workspace View',
		alias: 'example.workspaceView.counter',
		element: () => import('./counter-workspace-view.js'),
		weight: 900,
		meta: {
			label: 'Counter',
			pathname: 'counter',
			icon: 'icon-lab',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	// Workspace Footer App
	{
		type: 'workspaceFooterApp',
		alias: 'example.workspaceFooterApp.counterStatus',
		name: 'Counter Status Footer App',
		element: () => import('./counter-status-footer-app.element.js'),
		weight: 900,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
```
{% endcode %}

## Integration Patterns

### Context Communication
All extensions communicate through the shared workspace context:

1. **Context provides state** - Centralized state management with observables
2. **Actions modify state** - Primary and secondary actions update the counter
3. **Views display state** - Reactive UI components show current values
4. **Footer apps monitor state** - Persistent status indicators

### Extension Relationships
- **Action Menu Items** extend **Workspace Actions** using `forWorkspaceActions`
- **All extensions** consume the **Workspace Context** for communication
- **Views and Footer Apps** observe context state for reactive updates

{% hint style="success" %}
This pattern creates a cohesive workspace extension ecosystem where all components work together through shared context communication.
{% endhint %}