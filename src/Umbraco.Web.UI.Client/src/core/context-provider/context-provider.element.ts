import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Provides a value to the context down the DOM tree.
 *
 * @remarks This element is a wrapper around the `provideContext` function.
 * @slot - The context will be available to all descendants given in the default slot.
 * @throws {Error} If the key property is not set.
 * @throws {Error} If the value property is not set.
 */
@customElement('umb-context-provider')
export class UmbContextProviderElement extends UmbLitElement {
	/**
	 * The value to provide to the context.
	 * @optional
	 */
	@property({ type: Object, attribute: false })
	create?: (host: UmbControllerHostElement) => unknown;

	/**
	 * The value to provide to the context.
	 * @required
	 */
	@property({ type: Object })
	value: unknown;

	/**
	 * The key to provide to the context.
	 * @required
	 */
	@property({ type: String })
	key!: string | UmbContextToken;

	connectedCallback() {
		super.connectedCallback();
		if (!this.key) {
			throw new Error('The key property is required.');
		}
		if (this.create) {
			this.value = this.create(this);
		} else if (!this.value) {
			throw new Error('The value property is required.');
		}
		this.provideContext(this.key, this.value);
	}

	render() {
		return html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-context-provider': UmbContextProviderElement;
	}
}
