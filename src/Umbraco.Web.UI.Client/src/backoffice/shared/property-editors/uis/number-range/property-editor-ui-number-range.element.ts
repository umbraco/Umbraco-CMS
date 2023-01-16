import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-number-range
 */
@customElement('umb-property-editor-ui-number-range')
export class UmbPropertyEditorUINumberRangeElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-number-range</div>`;
	}
}

export default UmbPropertyEditorUINumberRangeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-number-range': UmbPropertyEditorUINumberRangeElement;
	}
}
