import { Observable } from 'rxjs';

import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/models';

import { UmbControllerHostElement, UmbControllerHostMixin } from '@umbraco-cms/backoffice/controller';
import {
	UmbContextToken,
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
} from '@umbraco-cms/backoffice/context-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

// TODO: can we use this aliases to generate the key of this type
interface ResolvedContexts {
	[key: string]: any;
}

export declare class UmbElementMixinInterface extends UmbControllerHostElement {
	observe<T>(source: Observable<T>, callback: (_value: T) => void, unique?: string): UmbObserverController<T>;
	provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R>;
	consumeContext<R = unknown>(
		alias: string | UmbContextToken<R>,
		callback: UmbContextCallback<R>
	): UmbContextConsumerController<R>;
	consumeAllContexts(contextAliases: string[], callback: (_instances: ResolvedContexts) => void): void;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostMixin(superClass) implements UmbElementMixinInterface {
		/**
		 * @description Observe a RxJS source of choice.
		 * @param {Observable<T>} source RxJS source
		 * @param {method} callback Callback method called when data is changed.
		 * @return {UmbObserverController} Reference to a Observer Controller instance
		 * @memberof UmbElementMixin
		 */
		observe<T>(source: Observable<T>, callback: (_value: T) => void, unique?: string): UmbObserverController<T> {
			return new UmbObserverController<T>(this, source, callback, unique);
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

		/**
		 * @description Setup a subscription for multiple contexts. The callback is called when all contexts are resolved.
		 * @param {string} aliases
		 * @param {method} callback Callback method called when all contexts are resolved.
		 * @memberof UmbElementMixin
		 * @deprecated it should not be necessary to consume multiple contexts at once, use consumeContext instead with an UmbContextToken
		 */
		consumeAllContexts(_contextAliases: Array<string>, callback: (_instances: ResolvedContexts) => void) {
			let resolvedAmount = 0;
			const controllers = _contextAliases.map(
				(alias) =>
					new UmbContextConsumerController(this, alias, () => {
						resolvedAmount++;

						if (resolvedAmount === _contextAliases.length) {
							const result: ResolvedContexts = {};

							controllers.forEach((contextCtrl: UmbContextConsumerController) => {
								result[contextCtrl.consumerAlias?.toString()] = contextCtrl.instance;
							});

							callback(result);
						}
					})
			);
		}
	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElementMixinInterface> & T;
};
