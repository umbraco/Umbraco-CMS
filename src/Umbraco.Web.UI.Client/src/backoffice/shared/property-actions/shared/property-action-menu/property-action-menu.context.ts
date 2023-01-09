import { BehaviorSubject } from 'rxjs';
import { UmbContextProviderController } from 'src/core/context-api/provide/context-provider.controller';
import type { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

export class UmbPropertyActionMenuContext {

	#isOpen = new BehaviorSubject(false);
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
