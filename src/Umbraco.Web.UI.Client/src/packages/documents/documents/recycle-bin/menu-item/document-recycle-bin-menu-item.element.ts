import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-recycle-bin-menu-item')
export class UmbDocumentRecycleMenuItemElement extends UmbLitElement {
	render() {
		return html`<umb-tree alias="Umb.Tree.DocumentRecycleBin"></umb-tree>`;
	}
}

export default UmbDocumentRecycleMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-recycle-bin-menu-item': UmbDocumentRecycleMenuItemElement;
	}
}
