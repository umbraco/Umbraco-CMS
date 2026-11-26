import type {
	UmbInputValuesWithLabelsElement,
	UmbValueWithLabelConfiguration,
} from './input-values-with-labels.element.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { umbBindToValidation, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import './input-values-with-labels.element.js';

/**
 * @element umb-property-editor-ui-values-with-labels
 */
@customElement('umb-property-editor-ui-values-with-labels')
export class UmbPropertyEditorUIValuesWithLabelsElement
	extends UmbFormControlMixin<Array<UmbValueWithLabelConfiguration>, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Makes the Values With Labels Property Editor UI readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Makes the Values With Labels Property Editor UI mandatory
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	required = false;

	#onChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UmbInputValuesWithLabelsElement;
		this.value = target.items;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-values-with-labels
				.items=${this.value ?? []}
				?readonly=${this.readonly}
				?required=${this.required}
				@change=${this.#onChange}
				${umbBindToValidation(this)}>
			</umb-input-values-with-labels>
		`;
	}
}

export default UmbPropertyEditorUIValuesWithLabelsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-values-with-labels': UmbPropertyEditorUIValuesWithLabelsElement;
	}
}
