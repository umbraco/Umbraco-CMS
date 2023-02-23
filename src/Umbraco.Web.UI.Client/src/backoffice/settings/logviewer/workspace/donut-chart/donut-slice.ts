import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-donut-slice')
export class UmbDonutSliceElement extends LitElement {
	@property({ type: Number })
	percent = 0;

	@property({ type: Number })
	amount = 0;

	@property()
	color = 'red';

	@property()
	name = '';

	willUpdate() {
		this.dispatchEvent(new CustomEvent('slice-update', { composed: true, bubbles: true }));
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-slice': UmbDonutSliceElement;
	}
}
