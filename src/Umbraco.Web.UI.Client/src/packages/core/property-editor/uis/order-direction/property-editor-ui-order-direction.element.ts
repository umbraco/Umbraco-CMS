import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-order-direction
 */
@customElement('umb-property-editor-ui-order-direction')
export class UmbPropertyEditorUIOrderDirectionElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = 'asc';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(e: UUIBooleanInputEvent) {
		this.value = e.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<uui-radio-group @input=${this.#onChange}>
			<uui-radio name="order" label="Ascending [a-z]" ?checked=${this.value === 'asc'} value="asc"></uui-radio>
			<uui-radio name="order" label="Descending [z-a]" ?checked=${this.value === 'desc'} value="desc"></uui-radio>
		</uui-radio-group>`;
	}

	static styles = [
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
