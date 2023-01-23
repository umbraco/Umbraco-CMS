import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbChangeEvent } from 'src/core/events/change.event';
import { UmbPropertyValueChangeEvent } from '../..';
import UmbInputMultipleTextStringElement, {
	MultipleTextStringValue,
} from './input-multiple-text-string/input-multiple-text-string.element';
import { UmbLitElement } from '@umbraco-cms/element';

import './input-multiple-text-string/input-multiple-text-string.element';

export type MultipleTextStringConfigData = Array<{
	alias: 'minNumber' | 'maxNumber';
	value: number;
}>;

/**
 * @element umb-property-editor-ui-multiple-text-string
 */
@customElement('umb-property-editor-ui-multiple-text-string')
export class UmbPropertyEditorUIMultipleTextStringElement extends UmbLitElement {
	static styles = [];

	@property({ type: Array })
	public value: MultipleTextStringValue = [];

	@property({ type: Array, attribute: false })
	public set config(config: MultipleTextStringConfigData) {
		this._limitMin = config.find((x) => x.alias === 'minNumber')?.value;
		this._limitMax = config.find((x) => x.alias === 'maxNumber')?.value;
	}

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

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
			?disabled=${this.disabled}></umb-input-multiple-text-string>`;
	}
}

export default UmbPropertyEditorUIMultipleTextStringElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-text-string': UmbPropertyEditorUIMultipleTextStringElement;
	}
}
