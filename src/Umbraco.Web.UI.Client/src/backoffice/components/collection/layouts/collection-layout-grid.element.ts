import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-layout-grid')
export class UmbCollectionLayoutGridElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`umb-collection-layout-grid`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-grid': UmbCollectionLayoutGridElement;
	}
}
