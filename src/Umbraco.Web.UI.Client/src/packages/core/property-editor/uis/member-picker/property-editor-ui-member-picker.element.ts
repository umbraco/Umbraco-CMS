import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-member-picker
 */
@customElement('umb-property-editor-ui-member-picker')
export class UmbPropertyEditorUIMemberPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html` <umb-input-member>Add</umb-input-member>`;
	}
}

export default UmbPropertyEditorUIMemberPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-picker': UmbPropertyEditorUIMemberPickerElement;
	}
}
