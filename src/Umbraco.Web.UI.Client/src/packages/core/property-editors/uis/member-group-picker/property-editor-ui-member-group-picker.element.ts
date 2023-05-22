import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-member-group-picker
 */
@customElement('umb-property-editor-ui-member-group-picker')
export class UmbPropertyEditorUIMemberGroupPickerElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = new UmbDataTypePropertyCollection();

	render() {
		return html`<div>umb-property-editor-ui-member-group-picker</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIMemberGroupPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-group-picker': UmbPropertyEditorUIMemberGroupPickerElement;
	}
}
