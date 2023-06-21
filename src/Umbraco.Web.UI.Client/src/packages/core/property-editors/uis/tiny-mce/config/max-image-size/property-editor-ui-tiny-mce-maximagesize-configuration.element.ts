import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-maximagesize-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-maximagesize-configuration')
export class UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement extends UmbLitElement {

	@property()
	value?: number;

	render() {
		return html`<uui-input type="number" placeholder="Max size" .value=${this.value}></uui-input>`;
	}
	
	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-maximagesize-configuration': UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;
	}
}
