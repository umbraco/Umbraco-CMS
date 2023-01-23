import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/context-api';

/**
 * @element umb-property-editor-ui-upload-field
 */
@customElement('umb-property-editor-ui-upload-field')
export class UmbPropertyEditorUIUploadFieldElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-upload-field</div>`;
	}
}

export default UmbPropertyEditorUIUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-upload-field': UmbPropertyEditorUIUploadFieldElement;
	}
}
