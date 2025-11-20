import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
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
				autocomplete="off"
				min="0"
				step="1"
				label=${this.localize.term('general_width')}
				placeholder=${this.localize.term('general_width')}
				@change=${this.#onChangeWidth}
				.value=${this.value?.width?.toString() ?? ''}>
			</uui-input>
			<span>&times;</span>
			<uui-input
				type="number"
				autocomplete="off"
				min="0"
				step="1"
				label=${this.localize.term('general_height')}
				placeholder=${this.localize.term('general_height')}
				@change=${this.#onChangeHeight}
				.value=${this.value?.height?.toString() ?? ''}>
			</uui-input>
			<umb-localize key="general_pixels">pixels</umb-localize>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-1);
			}
		`,
	];
}

export { UmbPropertyEditorUIDimensionsElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dimensions': UmbPropertyEditorUIDimensionsElement;
	}
}
