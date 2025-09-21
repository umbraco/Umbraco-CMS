import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-menu')
export class UmbDefaultCollectionMenuElement extends UmbLitElement {
	override render() {
		return html`<div>Hello from default collection menu</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-menu': UmbDefaultCollectionMenuElement;
	}
}
