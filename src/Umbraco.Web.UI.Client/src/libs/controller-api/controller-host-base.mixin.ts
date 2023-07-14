import { ClassConstructor } from '../extension-api/types.js';
import { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';

declare class UmbControllerHostBaseDeclaration implements Omit<UmbControllerHost, 'getHostElement'> {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(unique: UmbController['controllerAlias']): void;
	removeController(controller: UmbController): void;

	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}

/**
 * This mixin enables a class to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbControllerHostBaseMixin = <T extends ClassConstructor<any>>(superClass: T) => {
	class UmbControllerHostBaseClass extends superClass {
		#controllers: UmbController[] = [];

		#attached = false;

		/**
		 * Tests if a controller is assigned to this element.
		 * @param {UmbController} ctrl
		 */
		hasController(ctrl: UmbController): boolean {
			return this.#controllers.indexOf(ctrl) !== -1;
		}

		/**
		 * Retrieve controllers matching a filter of this element.
		 * @param {method} filterMethod
		 */
		getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[] {
			return this.#controllers.filter(filterMethod);
		}

		/**
		 * Append a controller to this element.
		 * @param {UmbController} ctrl
		 */
		addController(ctrl: UmbController): void {
			// If this specific class is already added, then skip out.
			if (this.#controllers.indexOf(ctrl) !== -1) {
				return;
			}

			// Check if there is one already with same unique
			this.removeControllerByAlias(ctrl.controllerAlias);

			this.#controllers.push(ctrl);
			if (this.#attached) {
				// If a controller is created on a already attached element, then it will be added directly. This might not be optimal. As the controller it self has not finished its constructor method jet. therefor i postpone the call:
				Promise.resolve().then(() => ctrl.hostConnected());
				//ctrl.hostConnected();
			}
		}

		/**
		 * Remove a controller from this element.
		 * Notice this will also destroy the controller.
		 * @param {UmbController} ctrl
		 */
		removeController(ctrl: UmbController): void {
			const index = this.#controllers.indexOf(ctrl);
			if (index !== -1) {
				this.#controllers.splice(index, 1);
				if (this.#attached) {
					ctrl.hostDisconnected();
				}
				ctrl.destroy();
			}
		}

		/**
		 * Remove a controller from this element by its alias.
		 * Notice this will also destroy the controller.
		 * @param {string | symbol} controllerAlias
		 */
		removeControllerByAlias(controllerAlias: UmbController['controllerAlias']): void {
			if (controllerAlias) {
				this.#controllers.forEach((x) => {
					if (x.controllerAlias === controllerAlias) {
						this.removeController(x);
					}
				});
			}
		}

		hostConnected() {
			this.#attached = true;
			this.#controllers.forEach((ctrl: UmbController) => ctrl.hostConnected());
		}

		hostDisconnected() {
			this.#attached = false;
			this.#controllers.forEach((ctrl: UmbController) => ctrl.hostDisconnected());
		}

		destroy() {
			this.#controllers.forEach((ctrl: UmbController) => ctrl.destroy());
			this.#controllers = [];
		}
	}

	return UmbControllerHostBaseClass as unknown as ClassConstructor<UmbControllerHostBaseDeclaration> & T;
};
