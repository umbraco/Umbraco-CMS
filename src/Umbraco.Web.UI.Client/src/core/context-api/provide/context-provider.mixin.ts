import type { HTMLElementConstructor } from '../../models';
import { UmbContextProvider } from './context-provider';

export declare class UmbContextProviderMixinInterface {
	provideContext(alias: string, instance: unknown): void;
}

export const UmbContextProviderMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbContextProviderClass extends superClass {
		_providers: Map<string, UmbContextProvider> = new Map();

		_attached = false;

		provideContext(alias: string, instance: unknown) {
			// TODO: Consider if its right to re-publish the context?
			const existingProvider = this._providers.get(alias);
			if (existingProvider) {
				existingProvider.hostDisconnected();
			}

			const provider = new UmbContextProvider(this, alias, instance);
			this._providers.set(alias, provider);
			// TODO: if already connected then attach the new one.
			if (this._attached) {
				provider.hostConnected();
			}
		}

		// TODO: unprovide method to enforce a detach?

		connectedCallback() {
			super.connectedCallback?.();
			this._attached = true;
			this._providers.forEach((provider) => provider.hostConnected());
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this._attached = false;
			this._providers.forEach((provider) => provider.hostDisconnected());
		}
	}

	return UmbContextProviderClass as unknown as HTMLElementConstructor<UmbContextProviderMixinInterface> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
