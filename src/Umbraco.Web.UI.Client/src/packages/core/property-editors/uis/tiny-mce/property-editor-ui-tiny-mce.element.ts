import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = new UmbDataTypePropertyCollection();

	render() {
		return html`<div>umb-property-editor-ui-tiny-mce</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
