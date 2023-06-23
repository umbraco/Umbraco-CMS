import { UmbControllerHostBaseMixin } from './controller-host-base.mixin.js';
import { UmbControllerHost } from './controller-host.interface.js';
import type { UmbControllerInterface } from './controller.interface.js';

type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

export declare class UmbControllerHostElement extends HTMLElement implements UmbControllerHost {
	hasController(controller: UmbControllerInterface): boolean;
	getControllers(filterMethod: (ctrl: UmbControllerInterface) => boolean): UmbControllerInterface[];
	addController(controller: UmbControllerInterface): void;
	removeControllerByUnique(unique: UmbControllerInterface['unique']): void;
	removeController(controller: UmbControllerInterface): void;
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
		connectedCallback() {
			super.connectedCallback();
			this.hostConnected();
		}

		disconnectedCallback() {
			super.disconnectedCallback();
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
