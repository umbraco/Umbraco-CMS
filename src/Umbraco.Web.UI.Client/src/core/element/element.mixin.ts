import { Observable } from 'rxjs';
import { UmbContextConsumerController } from '../context-api/consume/context-consumer.controller';
import { UmbContextCallback } from '../context-api/consume/context-request.event';
import { UmbContextProviderController } from '../context-api/provide/context-provider.controller';
import { UmbControllerHostInterface, UmbControllerHostMixin } from '../controller/controller-host.mixin';
import type { HTMLElementConstructor } from '../models';
import { UmbObserverController } from '../observable-api/observer.controller';

// TODO: can we use this aliases to generate the key of this type
interface ResolvedContexts {
	[key: string]: any;
}

export declare class UmbElementMixinInterface extends UmbControllerHostInterface {
	observe<T>(source: Observable<T>, callback: (_value: T) => void): UmbObserverController<T>;
	provideContext(alias: string, instance: unknown): UmbContextProviderController;
	consumeContext(alias: string, callback: UmbContextCallback): UmbContextConsumerController;
	consumeAllContexts(contextAliases: string[], callback: (_instances: ResolvedContexts) => void): void;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostMixin(superClass) {

		/**
		 * @description Observe a RxJS source of choice.
		 * @param {Observable<T>} source RxJS source
		 * @param {method} callback Callback method called when data is changed.
		 * @return {UmbObserverController} Reference to a Observer Controller instance
		 * @memberof UmbElementMixin
		 */
		observe<T>(source: Observable<T>, callback: (_value: T) => void): UmbObserverController<T> {
			return new UmbObserverController<T>(this, source, callback);
		}

		/**
		 * @description Provide a context API for this or child elements.
		 * @param {string} alias
		 * @param {instance} instance The API instance to be exposed.
		 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
		 * @memberof UmbElementMixin
		 */
		provideContext(alias: string, instance: unknown): UmbContextProviderController {
			return new UmbContextProviderController(this, alias, instance);
		}

		/**
		 * @description Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
		 * @memberof UmbElementMixin
		 */
		consumeContext(alias: string, callback: UmbContextCallback): UmbContextConsumerController {
			return new UmbContextConsumerController(this, alias, callback);
		}

		/**
		 * @description Setup a subscription for multiple contexts. The callback is called when all contexts are resolved.
		 * @param {string} aliases
		 * @param {method} callback Callback method called when all contexts are resolved.
		 * @memberof UmbElementMixin
		 */
		consumeAllContexts(_contextAliases: Array<string>, callback: (_instances: ResolvedContexts) => void) {
			let resolvedAmount = 0;
			const controllers = _contextAliases.map((alias) =>
				new UmbContextConsumerController(this, alias, () => {

					resolvedAmount++;

					if (resolvedAmount === _contextAliases.length) {

						const result: ResolvedContexts = {};

						controllers.forEach((contextCtrl: UmbContextConsumerController) => {
							result[contextCtrl.consumerAlias] = contextCtrl.instance;
						});

						callback(result);
					}
				})
			);
		}


	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElementMixinInterface> & T;
};
