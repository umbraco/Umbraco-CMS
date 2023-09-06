import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tags-storage-type
 */
@customElement('umb-property-editor-ui-tags-storage-type')
export class UmbPropertyEditorUITagsStorageTypeElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-tags-storage-type</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITagsStorageTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags-storage-type': UmbPropertyEditorUITagsStorageTypeElement;
	}
}
