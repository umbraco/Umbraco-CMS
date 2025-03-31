import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-dimensions
 */
@customElement('umb-property-editor-ui-dimensions')
export class UmbPropertyEditorUIDimensionsElement extends UmbLitElement {
	@property({ type: Object })
	value: { width?: number; height?: number } = {};

	#onChangeWidth(e: UUIInputEvent) {
		this.value = { ...this.value, width: Number(e.target.value as string) };
		this.dispatchEvent(new UmbChangeEvent());
	}
	#onChangeHeight(e: UUIInputEvent) {
		this.value = { ...this.value, height: Number(e.target.value as string) };
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				type="number"
				label=${this.localize.term('general_width')}
				placeholder=${this.localize.term('general_width')}
				@change=${this.#onChangeWidth}
				.value=${this.value?.width?.toString() ?? ''}>
			</uui-input>
			<span>&times;</span>
			<uui-input
				type="number"
				label=${this.localize.term('general_height')}
				placeholder=${this.localize.term('general_height')}
				@change=${this.#onChangeHeight}
				.value=${this.value?.height?.toString() ?? ''}>
			</uui-input>
			<umb-localize key="general_pixels">pixels</umb-localize>
		`;
	}

	static override readonly styles = [UmbTextStyles];
}

export { UmbPropertyEditorUIDimensionsElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dimensions': UmbPropertyEditorUIDimensionsElement;
	}
}
