import type { UmbControllerAlias } from './controller-alias.type.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';

export interface UmbControllerHostElement extends HTMLElement, UmbControllerHost {
	hasUmbController(controller: UmbController): boolean;
	getUmbControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addUmbController(controller: UmbController): void;
	removeUmbControllerByAlias(alias: UmbControllerAlias): void;
	removeUmbController(controller: UmbController): void;
	getHostElement(): Element;

	destroy(): void;
}
