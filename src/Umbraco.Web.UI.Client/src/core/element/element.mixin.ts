import { UmbContextConsumerController } from '../context-api/consume/context-consumer.controller';
import { isContextConsumerType } from '../context-api/consume/is-context-consumer-type.function';
import { UmbControllerHostInterface, UmbControllerHostMixin } from '../controller/controller-host.mixin';
import { UmbControllerInterface } from '../controller/controller.interface';
import type { HTMLElementConstructor } from '../models';

// TODO: can we use this aliases to generate the key of this type
interface ResolvedContexts {
	[key: string]: any;
}

export declare class UmbElementMixinInterface extends UmbControllerHostInterface {
	consumeContext(alias: string, callback: (_instance: any) => void): void;
	consumeAllContexts(contextAliases: string[], callback: (_instances: ResolvedContexts) => void): void;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostMixin(superClass) {

        
		/**
		 * Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 */
		consumeContext(alias: string, callback: (_instance: any) => void): void {
			new UmbContextConsumerController(this, alias, callback);
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
