import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-layout-table')
export class UmbCollectionLayoutTableElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`umb-collection-layout-table`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-table': UmbCollectionLayoutTableElement;
	}
}
