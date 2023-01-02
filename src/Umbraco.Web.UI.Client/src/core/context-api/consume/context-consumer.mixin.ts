import type { HTMLElementConstructor } from '../../models';
import { UmbContextConsumer } from './context-consumer';

export declare class UmbContextConsumerInterface {
	consumeContext(alias: string, callback: (_instance: any) => void): void;
	consumeAllContexts(contextAliases: string[], callback: (_instances: ResolvedContexts) => void): void;
	whenAvailableOrChanged(contextAliases: string[], callback?: () => void): void;
}

// TODO: can we use this aliases to generate the key of this type
interface ResolvedContexts {
	[key: string]: any;
}

/**
 * This mixin enables the component to consume contexts.
 * This is done by calling the `consumeContext` method.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbContextConsumerMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbContextConsumerClass extends superClass {
		// all context requesters in the element
		_consumers: Map<string, UmbContextConsumer[]> = new Map();
		// all successfully resolved context requests
		_resolved: Map<string, any> = new Map();

		_attached = false;

		/**
		 * Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 */
		consumeContext(alias: string, callback: (_instance: any) => void): void {
			this._createContextConsumers([alias], (resolvedContexts) => {
				callback(resolvedContexts[alias]);
			});
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
			aliases.forEach((alias) => {
				const consumer = new UmbContextConsumer(this, alias, (_instance: any) => {
					this._resolved.set(alias, _instance);

					const result: ResolvedContexts = {};

					//check if all contexts are resolved
					const resolvedContexts = aliases.map((alias) => (result[alias] = this._resolved.get(alias)));
					const allResolved = resolvedContexts.every((context) => context !== undefined);

					if (allResolved) {
						resolvedCallback(result);
					}
				});

				if (this._consumers.has(alias)) {
					const consumers = this._consumers.get(alias);
					consumers?.push(consumer);
				} else {
					this._consumers.set(alias, [consumer]);
				}

				if (this._attached) {
					consumer.hostConnected();
				}
			});
		}

		// TODO: remove requester..

		connectedCallback() {
			super.connectedCallback?.();
			this._attached = true;
			this._consumers.forEach((consumers) => consumers.forEach((consumer) => consumer.hostConnected()));
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this._attached = false;
			this._consumers.forEach((consumers) => consumers.forEach((consumer) => consumer.hostDisconnected()));
			this._resolved.clear();
		}

		// might return a object, so you can unsubscribe.
		whenAvailableOrChanged(_contextAliases: string[]) {
			// TODO: To be done.
		}
	}

	return UmbContextConsumerClass as unknown as HTMLElementConstructor<UmbContextConsumerInterface> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
