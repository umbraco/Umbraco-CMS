import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-tiny-mce-maximagesize
 */
@customElement('umb-property-editor-ui-tiny-mce-maximagesize')
export class UmbPropertyEditorUITinyMceMaxImageSizeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Number })
	value: number = 0;

	#onChange(e: UUIInputEvent) {
		this.value = Number(e.target.value as string);
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				type="number"
				min="0"
				label="Max size"
				placeholder="Max size"
				.value=${this.value}
				@change=${this.#onChange}>
			</uui-input>
		`;
	}
}

export default UmbPropertyEditorUITinyMceMaxImageSizeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-maximagesize': UmbPropertyEditorUITinyMceMaxImageSizeElement;
	}
}
