import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

/**
 * @element umb-property-editor-ui-image-crops-configuration
 */
@customElement('umb-property-editor-ui-image-crops-configuration')
export class UmbPropertyEditorUIImageCropsConfigurationElement extends LitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-image-crops-configuration</div>`;
	}
}

export default UmbPropertyEditorUIImageCropsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-crops-configuration': UmbPropertyEditorUIImageCropsConfigurationElement;
	}
}
