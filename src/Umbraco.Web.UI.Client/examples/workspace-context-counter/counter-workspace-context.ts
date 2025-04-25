import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

// The Example Workspace Context Controller:
export class WorkspaceContextCounterElement extends UmbContextBase {
	// We always keep our states private, and expose the values as observables:
	#counter = new UmbNumberState(0);
	readonly counter = this.#counter.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, EXAMPLE_COUNTER_CONTEXT);
	}

	// Lets expose methods to update the state:
	increment() {
		this.#counter.setValue(this.#counter.value + 1);
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = WorkspaceContextCounterElement;

// Declare a Context Token that other elements can use to request the WorkspaceContextCounter:
export const EXAMPLE_COUNTER_CONTEXT = new UmbContextToken<WorkspaceContextCounterElement>(
	'UmbWorkspaceContext',
	'example.workspaceContext.counter',
);
