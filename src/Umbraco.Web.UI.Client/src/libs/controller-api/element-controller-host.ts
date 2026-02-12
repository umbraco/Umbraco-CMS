import { UmbControllerHostMixin } from './controller-host.mixin.js';

/**
 * A minimal UmbControllerHost adapter that wraps a plain DOM element.
 * This enables creating contexts (like UmbEntityContext) that auto-provide
 * on the given element without requiring it to be a UmbControllerHostElement.
 */
export class UmbElementControllerHost extends UmbControllerHostMixin(Object) {
	#element: Element;

	constructor(element: Element) {
		super();
		this.#element = element;
	}

	getHostElement(): Element {
		return this.#element;
	}
}
