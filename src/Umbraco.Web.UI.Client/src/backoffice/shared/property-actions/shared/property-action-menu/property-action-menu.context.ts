import { UmbContextProviderController } from '@umbraco-cms/context-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { DeepState } from '@umbraco-cms/observable-api';

export class UmbPropertyActionMenuContext {
	#isOpen = new DeepState(false);
	public readonly isOpen = this.#isOpen.asObservable();

	constructor(host: UmbControllerHostInterface) {
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
