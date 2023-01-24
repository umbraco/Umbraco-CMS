import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from './lit-element.element';

@customElement('umb-context-provider')
export class UmbContextProviderElement extends UmbLitElement {
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
	key!: string;

	connectedCallback() {
		super.connectedCallback();
		if (!this.key) {
			throw new Error('The key property is required.');
		}
		if (!this.value) {
			throw new Error('The value property is required.');
		}
		this.provideContext(this.key, this.value);
	}

	render() {
		return html`<slot></slot>`;
	}
}
