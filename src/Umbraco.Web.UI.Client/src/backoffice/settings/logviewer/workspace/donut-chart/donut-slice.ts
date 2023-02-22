import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-donut-slice')
export class UmbDonutSliceElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Number })
	percent = 0;

	@property()
	tooltipText = '';

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
