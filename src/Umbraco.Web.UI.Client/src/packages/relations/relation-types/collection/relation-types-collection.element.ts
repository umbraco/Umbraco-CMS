import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-relation-types-collection')
export class UmbRelationTypesCollectionElement extends UmbCollectionDefaultElement {

	// NOTE: Returns empty toolbar, so to remove the header padding.
	protected renderToolbar() {
		return html``;
	}
}

export default UmbRelationTypesCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-types-collection': UmbRelationTypesCollectionElement;
	}
}
