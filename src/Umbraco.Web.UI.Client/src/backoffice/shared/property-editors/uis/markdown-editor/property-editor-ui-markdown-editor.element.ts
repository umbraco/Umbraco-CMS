import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

/**
 * @element umb-property-editor-ui-markdown-editor
 */
@customElement('umb-property-editor-ui-markdown-editor')
export class UmbPropertyEditorUIMarkdownEditorElement extends LitElement {
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
