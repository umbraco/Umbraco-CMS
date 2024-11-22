import type { UmbController } from './controller.interface.js';

export interface UmbControllerHost {
	hasUmbController(controller: UmbController): boolean;
	getUmbControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addUmbController(controller: UmbController): void;
	removeUmbControllerByAlias(unique: UmbController['controllerAlias']): void;
	removeUmbController(controller: UmbController): void;
	getHostElement(): Element;
}
