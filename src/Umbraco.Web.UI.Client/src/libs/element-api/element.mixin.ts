import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbContextToken,
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
} from '@umbraco-cms/backoffice/context-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export declare class UmbElement extends UmbControllerHostElement {
	observe<T>(
		source: Observable<T> | { asObservable: () => Observable<T> },
		callback: (_value: T) => void,
		unique?: string
	): UmbObserverController<T>;
	provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R>;
	consumeContext<R = unknown>(
		alias: string | UmbContextToken<R>,
		callback: UmbContextCallback<R>
	): UmbContextConsumerController<R>;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostElementMixin(superClass) implements UmbElement {
		/**
		 * @description Observe a RxJS source of choice.
		 * @param {Observable<T>} source RxJS source
		 * @param {method} callback Callback method called when data is changed.
		 * @return {UmbObserverController} Reference to a Observer Controller instance
		 * @memberof UmbElementMixin
		 */
		observe<T>(
			source: Observable<T> | { asObservable: () => Observable<T> },
			callback: (_value: T) => void,
			unique?: string
		) {
			return new UmbObserverController<T>(
				this,
				(source as any).asObservable ? (source as any).asObservable() : source,
				callback,
				unique
			);
		}

		/**
		 * @description Provide a context API for this or child elements.
		 * @param {string} alias
		 * @param {instance} instance The API instance to be exposed.
		 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
		 * @memberof UmbElementMixin
		 */
		provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R> {
			return new UmbContextProviderController(this, alias, instance);
		}

		/**
		 * @description Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
		 * @memberof UmbElementMixin
		 */
		consumeContext<R = unknown>(
			alias: string | UmbContextToken<R>,
			callback: UmbContextCallback<R>
		): UmbContextConsumerController<R> {
			return new UmbContextConsumerController(this, alias, callback);
		}
	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElement> & T;
};
