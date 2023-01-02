import { Observable } from 'rxjs';
import { UmbContextConsumerController } from '../context-api/consume/context-consumer.controller';
import { isContextConsumerType } from '../context-api/consume/is-context-consumer-type.function';
import { UmbContextProviderController } from '../context-api/provide/context-provider.controller';
import { UmbControllerHostInterface, UmbControllerHostMixin } from '../controller/controller-host.mixin';
import { UmbControllerInterface } from '../controller/controller.interface';
import type { HTMLElementConstructor } from '../models';
import { UmbObserverController } from '../observable-api/observer.controller';

// TODO: can we use this aliases to generate the key of this type
interface ResolvedContexts {
	[key: string]: any;
}

export declare class UmbElementMixinInterface extends UmbControllerHostInterface {
	observe<Y = any>(source: Observable<any>, callback: (_value: Y) => void): UmbObserverController;
	provideContext(alias: string, instance: unknown): UmbContextProviderController;
	consumeContext(alias: string, callback: (_instance: any) => void): UmbContextConsumerController;
	consumeAllContexts(contextAliases: string[], callback: (_instances: ResolvedContexts) => void): void;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostMixin(superClass) {

		/**
		 * Observe a RxJS source of choice.
		 * @param {string} alias
		 * @param {method} callback Callback method called when data is changed.
		 */
		observe<Y = any>(source: Observable<any>, callback: (_value: Y) => void): UmbObserverController {
			return new UmbObserverController(this, source, callback);
		}

		/**
		 * Provide a context API for this or child elements.
		 * @param {string} alias
		 * @param {instance} instance The API instance to be exposed.
		 */
		provideContext(alias: string, instance: unknown): UmbContextProviderController {
			return new UmbContextProviderController(this, alias, instance);
		}
        
		/**
		 * Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 */
		consumeContext(alias: string, callback: (_instance: any) => void): UmbContextConsumerController {
			return new UmbContextConsumerController(this, alias, callback);
		}

		/**
		 * Setup a subscription for multiple contexts. The callback is called when all contexts are resolved.
		 * @param {string} aliases
		 * @param {method} callback Callback method called when all contexts are resolved.
		 */
		consumeAllContexts(_contextAliases: Array<string>, callback: (_instances: ResolvedContexts) => void) {
			this._createContextConsumers(_contextAliases, (resolvedContexts) => {
				callback?.(resolvedContexts);
			});
		}



		private _createContextConsumers(aliases: Array<string>, resolvedCallback: (_instances: ResolvedContexts) => void) {
			aliases.forEach((alias) => 
				new UmbContextConsumerController(this, alias, () => {
					
                    const allResolved = this.getControllers((ctrl: UmbControllerInterface):boolean => isContextConsumerType(ctrl) && aliases.indexOf(ctrl.consumerAlias) !== -1 );

					if (allResolved.length === aliases.length) {
						resolvedCallback(allResolved);
					}
				})
			);
		}

	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElementMixinInterface> & T;
};
