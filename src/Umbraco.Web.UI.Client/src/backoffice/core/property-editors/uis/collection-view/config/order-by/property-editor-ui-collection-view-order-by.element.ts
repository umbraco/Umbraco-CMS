import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-collection-view-order-by
 */
@customElement('umb-property-editor-ui-collection-view-order-by')
export class UmbPropertyEditorUICollectionViewOrderByElement extends UmbLitElement {
	

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-collection-view-order-by</div>`;
	}
	
	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUICollectionViewOrderByElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-order-by': UmbPropertyEditorUICollectionViewOrderByElement;
	}
}
