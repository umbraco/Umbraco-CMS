import { type ObserverCallback, UmbObserver } from './observer.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbController, UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbObserverController<T = unknown> extends UmbObserver<T> implements UmbController {
	#host: UmbControllerHost;
	#alias?: UmbControllerAlias;

	public get controllerAlias(): UmbControllerAlias {
		return this.#alias;
	}

	constructor(
		host: UmbControllerHost,
		source: Observable<T>,
		callback?: ObserverCallback<T>,
		alias?: UmbControllerAlias,
	) {
		super(source, callback);
		this.#host = host;
		this.#alias = alias;

		// Lets check if controller is already here:
		// No we don't want this, as multiple different controllers might be looking at the same source.
		/*
		if (this._subscriptions.has(source)) {
			const subscription = this._subscriptions.get(source);
			subscription?.unsubscribe();
		}
		*/

		host.addUmbController(this);
	}

	override destroy(): void {
		this.#host?.removeUmbController(this);
		(this.#host as unknown) = undefined;
		super.destroy();
	}
}
