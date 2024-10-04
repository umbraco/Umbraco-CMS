import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-tiny-mce-dimensions-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-dimensions-configuration')
export class UmbPropertyEditorUITinyMceDimensionsConfigurationElement extends UmbLitElement {
	@property({ type: Object })
	value: { width?: number; height?: number } = {};

	#onChangeWidth(e: UUIInputEvent) {
		this.value = { ...this.value, width: Number(e.target.value as string) };
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
	#onChangeHeight(e: UUIInputEvent) {
		this.value = { ...this.value, height: Number(e.target.value as string) };
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<uui-input
				type="number"
				label=${this.localize.term('general_width')}
				placeholder=${this.localize.term('general_width')}
				@change=${this.#onChangeWidth}
				.value=${this.value?.width}></uui-input>
			x
			<uui-input
				type="number"
				label=${this.localize.term('general_height')}
				placeholder=${this.localize.term('general_height')}
				@change=${this.#onChangeHeight}
				.value=${this.value?.height}></uui-input>
			pixels`;
	}

	static override readonly styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITinyMceDimensionsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-dimensions-configuration': UmbPropertyEditorUITinyMceDimensionsConfigurationElement;
	}
}
