import type { UmbControllerHostElement } from './controller-host-element.interface.js';
import { UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-controller-host-provider')
export class UmbControllerHostProviderElement
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

	override connectedCallback() {
		super.connectedCallback();
		if (this.create) {
			this.create(this);
		}
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-controller-host-provider': UmbControllerHostProviderElement;
	}
}
