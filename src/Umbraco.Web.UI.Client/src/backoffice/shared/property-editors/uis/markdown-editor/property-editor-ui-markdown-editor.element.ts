import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-markdown-editor
 */
@customElement('umb-property-editor-ui-markdown-editor')
export class UmbPropertyEditorUIMarkdownEditorElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-markdown-editor</div>`;
	}
}

export default UmbPropertyEditorUIMarkdownEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-markdown-editor': UmbPropertyEditorUIMarkdownEditorElement;
	}
}
