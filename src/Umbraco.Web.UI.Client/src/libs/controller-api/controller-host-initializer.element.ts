import { UmbControllerHostElement, UmbControllerHostElementMixin } from './controller-host-element.mixin.js';

export class UmbControllerHostInitializerElement
	extends UmbControllerHostElementMixin(HTMLElement)
	implements UmbControllerHostElement
{
	/**
	 * A way to initialize controllers.
	 * @required
	 */
	create?: (host: UmbControllerHostElement) => void;

	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		const slot = document.createElement('slot');
		this.shadowRoot?.appendChild(slot);
	}

	connectedCallback() {
		super.connectedCallback();
		if (this.create) {
			this.create(this);
		}
	}
}

customElements.define('umb-controller-host-initializer', UmbControllerHostInitializerElement);

declare global {
	interface HTMLElementTagNameMap {
		'umb-controller-host-initializer': UmbControllerHostInitializerElement;
	}
}
