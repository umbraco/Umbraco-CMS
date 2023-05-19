import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-ui-overlay-size
 */
@customElement('umb-property-editor-ui-overlay-size')
export class UmbPropertyEditorUIOverlaySizeElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = new UmbDataTypePropertyCollection();
	
	render() {
		return html`<div>umb-property-editor-ui-overlay-size</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIOverlaySizeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-overlay-size': UmbPropertyEditorUIOverlaySizeElement;
	}
}
