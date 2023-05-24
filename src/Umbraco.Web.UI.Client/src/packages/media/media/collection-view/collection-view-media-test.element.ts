import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-collection-view-media-test')
export class UmbCollectionViewMediaTestElement extends LitElement {
	render() {
		return html`umb-collection-view-media-test`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media-test': UmbCollectionViewMediaTestElement;
	}
}
