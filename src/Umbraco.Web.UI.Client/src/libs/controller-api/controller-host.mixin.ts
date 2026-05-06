import type { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

interface UmbControllerHostBaseDeclaration extends Omit<UmbControllerHost, 'getHostElement'> {
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}

/**
 * This mixin enables a class to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 * Internal storage uses TypeScript `private` + lazy `??=` initialization — the
 * same pattern Lit's ReactiveElement uses for its own ReactiveController list.
 * This allows `addUmbController` to be called during `addInitializer`
 * callbacks (which run before subclass class-field initializers), mirroring
 * Lit's behavior.
 * @param {object} superClass - superclass to be extended.
 * @mixin
 * @returns {UmbControllerHost} - A class which extends the given superclass.
 */
export const UmbControllerHostMixin = <T extends ClassConstructor>(superClass: T) => {
	class UmbControllerHostBaseClass extends superClass implements UmbControllerHostBaseDeclaration {
		// Declared as TS `private` without an initializer so access is safe
		// before the class's class-field initializers run (i.e. during Lit's
		// `addInitializer` callbacks which fire inside `super()`).
		private _controllers?: UmbController[];
		private _attached?: boolean;

		getHostElement() {
			return undefined as any;
		}

		/**
		 * Tests if a controller is assigned to this element.
		 * @param {UmbController} ctrl - The controller to check for.
		 * @returns {boolean} - true if the controller is assigned
		 */
		hasUmbController(ctrl: UmbController): boolean {
			return (this._controllers?.indexOf(ctrl) ?? -1) !== -1;
		}

		/**
		 * Retrieve controllers matching a filter of this element.
		 * @param {Function} filterMethod - filter method
		 * @returns {Array<UmbController>} - currently assigned controllers passing the filter method.
		 */
		getUmbControllers(filterMethod: (ctrl: UmbController) => boolean): Array<UmbController> {
			return this._controllers?.filter(filterMethod) ?? [];
		}

		/**
		 * Append a controller to this element.
		 * @param {UmbController} ctrl - the controller to append to this host.
		 */
		addUmbController(ctrl: UmbController): void {
			const controllers = (this._controllers ??= []);

			// If this specific class is already added, then skip out.
			if (controllers.indexOf(ctrl) !== -1) {
				return;
			}

			// Check if there is one already with same unique
			this.removeUmbControllerByAlias(ctrl.controllerAlias);

			controllers.push(ctrl);
			if (this._attached) {
				// If a controller is created on a already attached element, then it will be added directly. This might not be optimal. As the controller it self has not finished its constructor method jet. therefor i postpone the call: [NL]
				Promise.resolve().then(() => {
					// Extra check to see if we are still attached and still added at this point:
					if (this._attached && this._controllers?.includes(ctrl)) {
						ctrl.hostConnected();
					}
				});
			}
		}

		/**
		 * Remove a controller from this element.
		 * Notice this will also destroy the controller.
		 * @param {UmbController} ctrl - The controller to remove and destroy from this host.
		 */
		removeUmbController(ctrl: UmbController): void {
			if (!this._controllers) return;
			const index = this._controllers.indexOf(ctrl);
			if (index !== -1) {
				this._controllers.splice(index, 1);
				if (this._attached) {
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
		removeUmbControllerByAlias(controllerAlias: UmbController['controllerAlias']): void {
			if (controllerAlias && this._controllers) {
				this._controllers.forEach((x) => {
					if (x.controllerAlias === controllerAlias) {
						this.removeUmbController(x);
					}
				});
			}
		}

		hostConnected(): void {
			this._attached = true;
			// Note: this might not be optimal, as if hostDisconnected remove one of the controllers, then the next controller will be skipped.
			this._controllers?.forEach((ctrl: UmbController) => ctrl.hostConnected());
		}

		hostDisconnected(): void {
			this._attached = false;
			// Note: this might not be optimal, as if hostDisconnected remove one of the controllers, then the next controller will be skipped.
			this._controllers?.forEach((ctrl: UmbController) => ctrl.hostDisconnected());
		}

		destroy(): void {
			if (!this._controllers) return;
			let ctrl: UmbController | undefined;
			let prev = null;
			// Note: A very important way of doing this loop, as foreach will skip over the next item if the current item is removed.
			while ((ctrl = this._controllers[0])) {
				ctrl.destroy();

				// Help developer realize that they made a mistake in code:
				if (ctrl === prev) {
					throw new Error(
						`Controller with controller alias: '${ctrl.controllerAlias?.toString()}' and class name: '${
							(ctrl as any).constructor.name
						}', does not remove it self when destroyed. This can cause memory leaks. Please fix this issue.\r\nThis usually occurs when you have a destroy() method that doesn't call super.destroy().`,
					);
				}
				prev = ctrl;
			}
			this._controllers.length = 0;
			this._attached = false;
		}
	}

	return UmbControllerHostBaseClass as unknown as ClassConstructor<UmbControllerHostBaseDeclaration> & T;
};
