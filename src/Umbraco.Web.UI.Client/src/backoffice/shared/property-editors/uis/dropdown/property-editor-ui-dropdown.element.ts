import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-dropdown
 */
@customElement('umb-property-editor-ui-dropdown')
export class UmbPropertyEditorUIDropdownElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-dropdown</div>`;
	}
}

export default UmbPropertyEditorUIDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dropdown': UmbPropertyEditorUIDropdownElement;
	}
}
