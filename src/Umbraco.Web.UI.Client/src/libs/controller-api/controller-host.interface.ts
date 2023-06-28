import type { UmbController } from './controller.interface.js';

export declare class UmbControllerHost {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByUnique(unique: UmbController['unique']): void;
	removeController(controller: UmbController): void;
	getElement(): EventTarget;
}
