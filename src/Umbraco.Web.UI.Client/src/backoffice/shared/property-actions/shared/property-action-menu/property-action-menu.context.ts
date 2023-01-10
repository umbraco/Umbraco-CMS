import { BehaviorSubject } from 'rxjs';
import { UmbContextProviderController } from 'src/core/context-api/provide/context-provider.controller';
import type { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

export class UmbPropertyActionMenuContext {

	private _isOpen = new BehaviorSubject(false);
	public readonly isOpen = this._isOpen.asObservable();

	constructor(host: UmbControllerHostInterface) {
		new UmbContextProviderController(host, 'umbPropertyActionMenu', this);
	}

	toggle() {
		this._isOpen.next(!this._isOpen.getValue());
	}
	open() {
		this._isOpen.next(true);
	}
	close() {
		this._isOpen.next(false);
	}
}
