import { ObserverCallback, UmbObserver } from './observer.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbController, UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbObserverController<T = unknown> extends UmbObserver<T> implements UmbController {
	_alias?: UmbControllerAlias;
	public get controllerAlias() {
		return this._alias;
	}

	constructor(
		host: UmbControllerHost,
		source: Observable<T>,
		callback: ObserverCallback<T>,
		alias?: UmbControllerAlias
	) {
		super(source, callback);
		this._alias = alias;

		// Lets check if controller is already here:
		// No we don't want this, as multiple different controllers might be looking at the same source.
		/*
		if (this._subscriptions.has(source)) {
			const subscription = this._subscriptions.get(source);
			subscription?.unsubscribe();
		}
		*/

		host.addController(this);
	}
}
