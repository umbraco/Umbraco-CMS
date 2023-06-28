import { HTMLElementConstructor } from '../extension-api/types.js';
import { UmbControllerHostBaseMixin } from './controller-host-base.mixin.js';
import { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';

export declare class UmbControllerHostElement extends HTMLElement implements UmbControllerHost {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByUnique(unique: UmbController['unique']): void;
	removeController(controller: UmbController): void;
	getElement(): EventTarget;
}

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbControllerHostMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbControllerHostClass extends UmbControllerHostBaseMixin(superClass) implements UmbControllerHost {
		getElement(): EventTarget {
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

	return UmbControllerHostClass as unknown as HTMLElementConstructor<UmbControllerHostElement> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
