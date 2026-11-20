import type {
	UmbInputValueListOptionsElement,
	UmbValueListOptionConfiguration,
} from './input-value-list-options.element.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { umbBindToValidation, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import './input-value-list-options.element.js';

/**
 * @element umb-property-editor-ui-value-list-options
 */
@customElement('umb-property-editor-ui-value-list-options')
export class UmbPropertyEditorUIValueListOptionsElement
	extends UmbFormControlMixin<Array<UmbValueListOptionConfiguration>, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Makes the Value List Options Property Editor UI readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Makes the Value List Options Property Editor UI mandatory
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	required = false;

	#onChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UmbInputValueListOptionsElement;
		this.value = target.items;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-value-list-options
				.items=${this.value ?? []}
				?readonly=${this.readonly}
				?required=${this.required}
				@change=${this.#onChange}
				${umbBindToValidation(this)}>
			</umb-input-value-list-options>
		`;
	}
}

export default UmbPropertyEditorUIValueListOptionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-value-list-options': UmbPropertyEditorUIValueListOptionsElement;
	}
}
