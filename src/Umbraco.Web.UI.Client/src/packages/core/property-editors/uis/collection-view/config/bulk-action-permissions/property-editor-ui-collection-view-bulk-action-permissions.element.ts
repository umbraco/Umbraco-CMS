import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
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
	
	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-bulk-action-permissions': UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;
	}
}
