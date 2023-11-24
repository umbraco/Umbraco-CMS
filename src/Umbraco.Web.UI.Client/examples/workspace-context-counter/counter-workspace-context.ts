import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

// The Example Workspace Context Controller:
export class WorkspaceContextCounter extends UmbBaseController {

	// We always keep our states private, and expose the values as observables:
	#counter = new UmbNumberState(0);
	readonly counter = this.#counter.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(EXAMPLE_COUNTER_CONTEXT, this);
		console.log("Hey new context")
	}

	// Lets expose methods to update the state:
	increment() {
		this.#counter.next(this.#counter.value + 1);
	}

}

// Declare a api export, so Extension Registry can initialize this class:
export const api = WorkspaceContextCounter;


// Declare a Context Token that other elements can use to request the WorkspaceContextCounter:
export const EXAMPLE_COUNTER_CONTEXT = new UmbContextToken<WorkspaceContextCounter>('example.workspaceContext.counter');
