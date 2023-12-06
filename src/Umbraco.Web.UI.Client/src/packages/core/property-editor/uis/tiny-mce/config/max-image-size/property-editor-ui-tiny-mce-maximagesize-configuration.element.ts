import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-tiny-mce-maximagesize-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-maximagesize-configuration')
export class UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Number })
	value: number = 0;

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
