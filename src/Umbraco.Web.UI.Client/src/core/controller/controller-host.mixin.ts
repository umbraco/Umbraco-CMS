import type { HTMLElementConstructor } from '../models';
import { UmbController } from './controller.interface';

export declare class UmbControllerHostInterface extends HTMLElement {
    //#controllers:UmbController[];
	//#attached:boolean;
	addController(controller:UmbController): void;
}

/**
 * This mixin enables the component to host controllers.
 * This is done by calling the `consumeContext` method.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbControllerHostMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbContextConsumerClass extends superClass {

		#controllers: UmbController[] = [];

		#attached = false;

		/**
		 * Append a controller to this element.
		 * @param {UmbController} ctrl
		 */
		addController(ctrl: UmbController): void {
			this.#controllers.push(ctrl);
            if(this.#attached) {
                ctrl.hostConnected();
            }
		}

		connectedCallback() {
			super.connectedCallback?.();
			this.#attached = true;
			this.#controllers.forEach((ctrl: UmbController) => ctrl.hostConnected());
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this.#attached = false;
			this.#controllers.forEach((ctrl: UmbController) => ctrl.hostDisconnected());
		}
	}

	return UmbContextConsumerClass as unknown as HTMLElementConstructor<UmbControllerHostInterface> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
