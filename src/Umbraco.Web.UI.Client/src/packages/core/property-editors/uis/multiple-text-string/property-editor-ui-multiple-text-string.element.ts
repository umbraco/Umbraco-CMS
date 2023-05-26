import { UmbPropertyValueChangeEvent } from '../../index.js';
import {
	UmbInputMultipleTextStringElement,
	MultipleTextStringValue,
} from './input-multiple-text-string/input-multiple-text-string.element.js';
import { html , customElement, property, state , ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-multiple-text-string
 */
@customElement('umb-property-editor-ui-multiple-text-string')
export class UmbPropertyEditorUIMultipleTextStringElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property({ type: Array })
	public value: MultipleTextStringValue = [];

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._limitMin = config.getValueByAlias('minNumber');
		this._limitMax = config.getValueByAlias('maxNumber');
	}

	/**
	 * Disables the Multiple Text String Property Editor UI
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Makes the Multiple Text String Property Editor UI readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Makes the Multiple Text String Property Editor UI mandatory
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	required = false;

	@state()
	private _limitMin?: number;

	@state()
	private _limitMax?: number;

	#onChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UmbInputMultipleTextStringElement;
		this.value = target.items;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-multiple-text-string
			.items="${this.value}"
			min="${ifDefined(this._limitMin)}"
			max="${ifDefined(this._limitMax)}"
			@change=${this.#onChange}
			?disabled=${this.disabled}
			?readonly=${this.readonly}
			?required=${this.required}></umb-input-multiple-text-string>`;
	}
}

export default UmbPropertyEditorUIMultipleTextStringElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-text-string': UmbPropertyEditorUIMultipleTextStringElement;
	}
}
