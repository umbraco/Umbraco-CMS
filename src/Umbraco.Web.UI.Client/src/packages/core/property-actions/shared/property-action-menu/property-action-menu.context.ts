import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeepState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPropertyActionMenuContext {
	#isOpen = new UmbDeepState(false);
	public readonly isOpen = this.#isOpen.asObservable();

	constructor(host: UmbControllerHostElement) {
		new UmbContextProviderController(host, 'umbPropertyActionMenu', this);
	}

	toggle() {
		this.#isOpen.next(!this.#isOpen.getValue());
	}
	open() {
		this.#isOpen.next(true);
	}
	close() {
		this.#isOpen.next(false);
	}
}
