import { Observable } from 'rxjs';
import { UmbObserver } from './observer';
import { UmbControllerInterface, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbObserverController<T = unknown> extends UmbObserver<T> implements UmbControllerInterface {
	_alias?: string;
	public get unique() {
		return this._alias;
	}

	constructor(host: UmbControllerHostElement, source: Observable<T>, callback: (_value: T) => void, alias?: string) {
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
