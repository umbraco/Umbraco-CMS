import { UmbControllerHostMixin } from './controller-host.mixin.js';

/**
 * A controller host backed by a DOM element.
 * Enables providing contexts and hosting controllers on elements that are not UmbLitElements.
 * @class UmbElementControllerHost
 */
export class UmbElementControllerHost extends UmbControllerHostMixin(class {}) {
	#element: Element;

	constructor(element: Element) {
		super();
		this.#element = element;
	}

	getHostElement(): Element {
		return this.#element;
	}
}
