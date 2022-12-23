import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-view-media-test')
export class UmbCollectionViewMediaTestElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`umb-collection-view-media-test`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media-test': UmbCollectionViewMediaTestElement;
	}
}
