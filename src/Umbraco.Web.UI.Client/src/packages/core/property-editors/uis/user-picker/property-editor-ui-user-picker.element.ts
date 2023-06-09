import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-user-picker
 */
@customElement('umb-property-editor-ui-user-picker')
export class UmbPropertyEditorUIUserPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = new UmbDataTypePropertyCollection();

	// TODO: implement config
	render() {
		return html` <umb-user-input></umb-user-input>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIUserPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-user-picker': UmbPropertyEditorUIUserPickerElement;
	}
}
