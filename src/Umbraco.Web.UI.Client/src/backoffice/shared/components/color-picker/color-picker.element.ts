import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIColorSwatchesEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-color-picker')
export class UmbColorPickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles];

	@property({ type: Boolean })
	showLabels = false;

	@property()
	colors?: string[];

	constructor() {
		super();
	}

	protected getFormElement() {
		return undefined;
	}

	private _onChange(e: UUIColorSwatchesEvent) {
		e.stopPropagation();
		super.value = e.target.value;
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`
			<uui-color-swatches @change="${this._onChange}" label="Color picker">${this._renderColors()} </uui-color-swatches>
		`;
	}

	private _renderColors() {
		return html`${this.colors?.map((color) => {
			return html`<uui-color-swatch
				label="${color}"
				value="${color}"
				.showLabel=${this.showLabels}></uui-color-swatch>`;
		})}`;
	}
}

export default UmbColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-color-picker': UmbColorPickerElement;
	}
}
