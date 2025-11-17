import { UMB_SORT_PROPERTY_CONTEXT } from './sort.property-context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSortPropertyContext extends UmbContextBase {
	#sortingMode = new UmbBooleanState(false);
	readonly sortingMode = this.#sortingMode.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_SORT_PROPERTY_CONTEXT);
	}

	getSortingMode(): boolean | undefined {
		return this.#sortingMode.getValue();
	}

	setSortingMode(sortingMode: boolean) {
		this.#sortingMode.setValue(sortingMode);
	}

	toggleSortingMode() {
		const enabled = this.getSortingMode();
		this.setSortingMode(!enabled);
	}
}

export { UmbSortPropertyContext as api };
