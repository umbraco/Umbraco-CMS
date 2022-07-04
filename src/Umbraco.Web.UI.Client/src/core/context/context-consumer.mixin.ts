import { HTMLElementConstructor } from '../models';
import { UmbContextConsumer } from './context-consumer';

export declare class UmbContextConsumerInterface {
	consumeContext(alias: string, callback?: (_instance: any) => void): void;
	whenAvailableOrChanged(contextAliases: string[], callback?: () => void): void;
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
		_consumers: Map<string, UmbContextConsumer> = new Map();
		// all successfully resolved context requests
		_resolved: Map<string, unknown> = new Map();

		_attached = false;

		/**
		 * Setup a subscription for a request on a given context of this component.
		 * @param {string} alias
		 * @param {method} callback optional callback method called when context is received or when context is detached.
		 */
		consumeContext(alias: string, callback?: (_instance: unknown) => void): void {
			if (this._consumers.has(alias)) return;

			const consumer = new UmbContextConsumer(this, alias, (_instance: any) => {
				// Do we still have this consumer?

				callback?.(_instance);

				// don't to anything if the context is already resolved
				if (this._resolved.has(alias) && this._resolved.get(alias) === _instance) return;

				this._resolved.set(alias, _instance);
				this._consumeContextCallback(alias, _instance);
			});

			this._consumers.set(alias, consumer);

			if (this._attached) {
				consumer.attach();
			}
		}

		// TODO: remove requester..

		connectedCallback() {
			super.connectedCallback?.();
			this._attached = true;
			this._consumers.forEach((requester) => requester.attach());
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this._attached = false;
			this._consumers.forEach((requester) => requester.detach());
			this._resolved.clear();
		}

		_consumeContextCallback(_newAlias: string, _newInstance: unknown) {
			// TODO: do be done.
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
