import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPropertyActionMenuContext extends UmbBaseController {
	#isOpen = new UmbBooleanState(false);
	public readonly isOpen = this.#isOpen.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext('umbPropertyActionMenu', this);
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
