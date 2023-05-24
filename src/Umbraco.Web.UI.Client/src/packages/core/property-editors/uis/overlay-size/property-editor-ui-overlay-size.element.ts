import { html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

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
