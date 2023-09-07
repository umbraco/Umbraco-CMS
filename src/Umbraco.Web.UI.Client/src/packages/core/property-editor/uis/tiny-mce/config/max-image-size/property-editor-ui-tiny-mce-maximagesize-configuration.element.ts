import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
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

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-maximagesize-configuration': UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;
	}
}
