import { UMB_PROPERTY_SORT_MODE_CONTEXT } from './property-sort-mode.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPropertySortModeContext extends UmbContextBase {
	#sortingMode = new UmbBooleanState(false);
	readonly sortingMode = this.#sortingMode.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_SORT_MODE_CONTEXT);
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

export { UmbPropertySortModeContext as api };
