import { Observable, ReplaySubject } from 'rxjs';
import { UmbContextProviderController } from 'src/core/context-api/provide/context-provider.controller';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

export class UmbPropertyActionMenuContext {

	private _isOpen: ReplaySubject<boolean> = new ReplaySubject(1);
	public readonly isOpen: Observable<boolean> = this._isOpen.asObservable();

	constructor(host: UmbControllerHostInterface) {
		new UmbContextProviderController(host, 'umbPropertyActionMenu', this);
	}

	open() {
		this._isOpen.next(true);
	}

	close() {
		this._isOpen.next(false);
	}
}
