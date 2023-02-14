import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-donut-slice')
export class UmbDonutSliceElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Number })
	percent = 0;

	@property()
	color = 'red';

	@property()
	name = '';
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-slice': UmbDonutSliceElement;
	}
}
