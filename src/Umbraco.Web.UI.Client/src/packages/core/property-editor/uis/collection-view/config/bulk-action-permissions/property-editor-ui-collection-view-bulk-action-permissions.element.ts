import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-collection-view-bulk-action-permissions
 */
@customElement('umb-property-editor-ui-collection-view-bulk-action-permissions')
export class UmbPropertyEditorUICollectionViewBulkActionPermissionsElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-collection-view-bulk-action-permissions</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-bulk-action-permissions': UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;
	}
}
