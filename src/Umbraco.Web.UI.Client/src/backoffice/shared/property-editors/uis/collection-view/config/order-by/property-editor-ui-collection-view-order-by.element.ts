import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

/**
 * @element umb-property-editor-ui-collection-view-order-by
 */
@customElement('umb-property-editor-ui-collection-view-order-by')
export class UmbPropertyEditorUICollectionViewOrderByElement extends LitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-collection-view-order-by</div>`;
	}
}

export default UmbPropertyEditorUICollectionViewOrderByElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-order-by': UmbPropertyEditorUICollectionViewOrderByElement;
	}
}
