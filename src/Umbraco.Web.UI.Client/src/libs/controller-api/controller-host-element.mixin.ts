import { UmbControllerHostMixin } from './controller-host.mixin.js';
import type { UmbControllerHostElement } from './controller-host-element.interface.js';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 * @param {object} superClass - superclass to be extended.
 * @mixin
 * @returns {HTMLElementConstructor<UmbControllerHostElement> & HTMLElementConstructor} - extended superclass.
 */
export const UmbControllerHostElementMixin = <T extends HTMLElementConstructor>(
	superClass: T,
): HTMLElementConstructor<UmbControllerHostElement> & T => {
	class UmbControllerHostElementClass
		extends UmbControllerHostMixin<T>(superClass)
		implements UmbControllerHostElement
	{
		getHostElement(): Element {
			return this;
		}

		override connectedCallback() {
			super.connectedCallback?.();
			this.hostConnected();
		}

		override disconnectedCallback() {
			super.disconnectedCallback?.();
			this.hostDisconnected();
		}
	}

	return UmbControllerHostElementClass as unknown as HTMLElementConstructor<UmbControllerHostElement> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
