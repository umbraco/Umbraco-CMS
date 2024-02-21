import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-tags-storage-type
 */
@customElement('umb-property-editor-ui-tags-storage-type')
export class UmbPropertyEditorUITagsStorageTypeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

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
