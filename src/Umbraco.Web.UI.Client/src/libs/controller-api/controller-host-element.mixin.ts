import type { UmbControllerAlias } from './controller-alias.type.js';
import { UmbControllerHostMixin } from './controller-host.mixin.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';

export declare class UmbControllerHostElement extends HTMLElement implements UmbControllerHost {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(alias: UmbControllerAlias): void;
	removeController(controller: UmbController): void;
	getHostElement(): Element;
}

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbControllerHostElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbControllerHostElementClass extends UmbControllerHostMixin(superClass) implements UmbControllerHost {
		getHostElement(): Element {
			return this;
		}

		connectedCallback() {
			super.connectedCallback?.();
			this.hostConnected();
		}

		disconnectedCallback() {
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
