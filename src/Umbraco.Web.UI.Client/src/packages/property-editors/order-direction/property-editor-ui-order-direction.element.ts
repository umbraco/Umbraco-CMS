import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-order-direction
 */
@customElement('umb-property-editor-ui-order-direction')
export class UmbPropertyEditorUIOrderDirectionElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = 'asc';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onInput(e: UUIBooleanInputEvent) {
		this.value = e.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<uui-radio-group @input=${this.#onInput} value=${this.value}>
			<uui-radio name="order" label="Ascending [a-z]" value="asc"></uui-radio>
			<uui-radio name="order" label="Descending [z-a]" value="desc"></uui-radio>
		</uui-radio-group>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-radio-group {
				display: flex;
				flex-direction: row;
				gap: var(--uui-size-6);
			}
		`,
	];
}

export default UmbPropertyEditorUIOrderDirectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-order-direction': UmbPropertyEditorUIOrderDirectionElement;
	}
}
