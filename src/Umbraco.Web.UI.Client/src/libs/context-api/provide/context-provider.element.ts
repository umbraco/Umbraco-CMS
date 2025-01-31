import type { UmbContextToken } from '../token/index.js';
import { UmbContextProviderController } from './context-provider.controller.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

/**
 * Provides a value to the context down the DOM tree.
 * @remarks This element is a wrapper around the `provideContext` function.
 * @slot - The context will be available to all descendants given in the default slot.
 * @throws {Error} If the key property is not set.
 * @throws {Error} If the value property is not set.
 */
export class UmbContextProviderElement extends UmbControllerHostElementMixin(HTMLElement) {
	/**
	 * The value to provide to the context.
	 * @optional
	 */
	public create?: (host: UmbControllerHostElement) => unknown;

	/**
	 * The value to provide to the context.
	 * @required
	 */
	public value: unknown;

	/**
	 * The key to provide to the context.
	 * @required
	 */
	public key!: string | UmbContextToken;

	static get observedAttributes() {
		return ['value', 'key'];
	}

	attributeChangedCallback(name: string, _oldValue: string | UmbContextToken, newValue: string | UmbContextToken) {
		if (name === 'key') this.key = newValue;
		if (name === 'value') this.value = newValue;
	}

	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		const slot = document.createElement('slot');
		this.shadowRoot?.appendChild(slot);
	}

	override connectedCallback() {
		super.connectedCallback();
		if (!this.key) {
			throw new Error('The key property is required.');
		}
		if (this.create) {
			this.value = this.create(this);
		} else if (!this.value) {
			throw new Error('The value property is required.');
		}

		new UmbContextProviderController(this, this.key, this.value);
	}
}

customElements.define('umb-context-provider', UmbContextProviderElement);

declare global {
	interface HTMLElementTagNameMap {
		'umb-context-provider': UmbContextProviderElement;
	}
}
