import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-layout-media-table')
export class UmbCollectionLayoutMediaTableElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<h1>umb-collection-layout-media-table</h1>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-media-table': UmbCollectionLayoutMediaTableElement;
	}
}
