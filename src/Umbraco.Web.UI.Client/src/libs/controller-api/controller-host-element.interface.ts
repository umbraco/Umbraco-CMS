import type { UmbControllerAlias } from './controller-alias.type.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';

export interface UmbControllerHostElement extends HTMLElement, UmbControllerHost {
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(alias: UmbControllerAlias): void;
	removeController(controller: UmbController): void;
	getHostElement(): Element;

	destroy(): void;
}
