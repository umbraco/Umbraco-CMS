import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-grid-stylesheet-picker
 */
@customElement('umb-property-editor-ui-block-grid-stylesheet-picker')
export class UmbPropertyEditorUIBlockGridStylesheetPickerElement extends UmbLitElement {
	

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-block-grid-stylesheet-picker</div>`;
	}
	
	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIBlockGridStylesheetPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-stylesheet-picker': UmbPropertyEditorUIBlockGridStylesheetPickerElement;
	}
}
