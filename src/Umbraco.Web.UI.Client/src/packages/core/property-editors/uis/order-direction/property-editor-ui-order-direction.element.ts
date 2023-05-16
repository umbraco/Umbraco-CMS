import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-order-direction
 */
@customElement('umb-property-editor-ui-order-direction')
export class UmbPropertyEditorUIOrderDirectionElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-order-direction</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIOrderDirectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-order-direction': UmbPropertyEditorUIOrderDirectionElement;
	}
}
