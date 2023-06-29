import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-markdown-editor
 */
@customElement('umb-property-editor-ui-markdown-editor')
export class UmbPropertyEditorUIMarkdownEditorElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbDataTypeConfigCollection;

	render() {
		return html`<div>umb-property-editor-ui-markdown-editor</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIMarkdownEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-markdown-editor': UmbPropertyEditorUIMarkdownEditorElement;
	}
}
