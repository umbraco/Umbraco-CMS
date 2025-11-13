import { UMB_SORT_PROPERTY_CONTEXT } from './sort.property-context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSortPropertyContext extends UmbContextBase {
	#enabled = new UmbBooleanState(false);
	readonly enabled = this.#enabled.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_SORT_PROPERTY_CONTEXT);
	}

	enable() {
		this.#enabled.setValue(true);
	}

	disable() {
		this.#enabled.setValue(false);
	}

	toggle() {
		const enabled = this.#enabled.getValue();
		if (enabled) {
			this.disable();
		} else {
			this.enable();
		}
	}
}

export { UmbSortPropertyContext as api };
