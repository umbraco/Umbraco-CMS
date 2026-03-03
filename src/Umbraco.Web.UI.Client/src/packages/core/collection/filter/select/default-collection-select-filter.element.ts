import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-select-filter')
export class UmbDefaultCollectionSelectFilterElement extends UmbLitElement {
	protected override render() {
		return html`<span>Select filter works!</span>`;
	}
}

export { UmbDefaultCollectionSelectFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-select-filter': UmbDefaultCollectionSelectFilterElement;
	}
}
