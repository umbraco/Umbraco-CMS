import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbPropertyValueChangeEvent } from '../..';
import UmbInputMultipleTextStringElement, {
	MultipleTextStringValue,
} from './input-multiple-text-string/input-multiple-text-string.element';
import { UmbPropertyEditorElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

export type MultipleTextStringConfigData = Array<{
	alias: 'minNumber' | 'maxNumber';
	value: number;
}>;

/**
 * @element umb-property-editor-ui-multiple-text-string
 */
@customElement('umb-property-editor-ui-multiple-text-string')
export class UmbPropertyEditorUIMultipleTextStringElement extends UmbLitElement implements UmbPropertyEditorElement {
	@property({ type: Array })
	public value: MultipleTextStringValue = [];

	@property({ type: Array, attribute: false })
	public set config(config: MultipleTextStringConfigData) {
		this._limitMin = config.find((x) => x.alias === 'minNumber')?.value;
		this._limitMax = config.find((x) => x.alias === 'maxNumber')?.value;
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
