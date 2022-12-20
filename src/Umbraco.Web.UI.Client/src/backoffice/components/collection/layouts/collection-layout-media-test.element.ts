import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-layout-media-test')
export class UmbCollectionLayoutMediaTestElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`umb-collection-layout-media-test`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-media-test': UmbCollectionLayoutMediaTestElement;
	}
}
