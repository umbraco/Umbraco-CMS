import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

/**
 * @element umb-property-editor-ui-tags-storage-type
 */
@customElement('umb-property-editor-ui-tags-storage-type')
export class UmbPropertyEditorUITagsStorageTypeElement extends LitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-tags-storage-type</div>`;
	}
}

export default UmbPropertyEditorUITagsStorageTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags-storage-type': UmbPropertyEditorUITagsStorageTypeElement;
	}
}
