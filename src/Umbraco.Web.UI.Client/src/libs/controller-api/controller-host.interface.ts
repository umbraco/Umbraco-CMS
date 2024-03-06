import type { UmbController } from './controller.interface.js';

export interface UmbControllerHost {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(unique: UmbController['controllerAlias']): void;
	removeController(controller: UmbController): void;
	getHostElement(): Element;
}
