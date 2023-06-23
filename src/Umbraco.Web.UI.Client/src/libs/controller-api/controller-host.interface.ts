import type { UmbControllerInterface } from './controller.interface.js';

export declare class UmbControllerHost {
	hasController(controller: UmbControllerInterface): boolean;
	getControllers(filterMethod: (ctrl: UmbControllerInterface) => boolean): UmbControllerInterface[];
	addController(controller: UmbControllerInterface): void;
	removeControllerByUnique(unique: UmbControllerInterface['unique']): void;
	removeController(controller: UmbControllerInterface): void;
}
