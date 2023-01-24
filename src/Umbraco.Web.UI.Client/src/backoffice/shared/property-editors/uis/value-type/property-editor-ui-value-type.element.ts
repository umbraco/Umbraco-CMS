import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-value-type
 */
@customElement('umb-property-editor-ui-value-type')
export class UmbPropertyEditorUIValueTypeElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-value-type</div>`;
	}
}

export default UmbPropertyEditorUIValueTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-value-type': UmbPropertyEditorUIValueTypeElement;
	}
}
