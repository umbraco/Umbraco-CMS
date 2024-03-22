import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

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

	#onChange(e: UUIInputEvent) {
		this.value = Number(e.target.value as string);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<uui-input
			label="Max size"
			type="number"
			placeholder="Max size"
			@change=${this.#onChange}
			.value=${this.value}></uui-input>`;
	}
}

export default UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-maximagesize-configuration': UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;
	}
}
