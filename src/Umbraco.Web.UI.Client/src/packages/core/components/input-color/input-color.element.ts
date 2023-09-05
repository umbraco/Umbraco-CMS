import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';

/*
 * This wraps the UUI library uui-color-swatches component
 * @element umb-input-color
 */
@customElement('umb-input-color')
export class UmbInputColorElement extends FormControlMixin(UmbLitElement) {
	@property({ type: Boolean })
	showLabels = false;

	@property({ type: Array })
	swatches?: UmbSwatchDetails[];

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
		return html`${this.swatches?.map((swatch) => {
			return html`<uui-color-swatch
				label="${swatch.label}"
				value="${swatch.value}"
				.showLabel=${this.showLabels}></uui-color-swatch>`;
		})}`;
	}
}

export default UmbInputColorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-color': UmbInputColorElement;
	}
}
