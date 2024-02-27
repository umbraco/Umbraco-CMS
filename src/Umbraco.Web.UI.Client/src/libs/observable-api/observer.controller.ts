import { type ObserverCallback, UmbObserver } from './observer.js';
import { simpleHashCode } from './utils/simple-hash-code.function.js';
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
		callback: ObserverCallback<T>,
		alias?: UmbControllerAlias,
	) {
		super(source, callback);
		this.#host = host;
		// Fallback to use a hash of the provided method:
		this.#alias = alias ?? simpleHashCode(callback.toString());

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

	destroy(): void {
		this.#host?.removeController(this);
		(this.#host as any) = undefined;
		super.destroy();
	}
}
