import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import UmbInputSliderElement from '../../../../shared/components/input-slider/input-slider.element';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

/**
 * @element umb-property-editor-ui-slider
 */
@customElement('umb-property-editor-ui-slider')
export class UmbPropertyEditorUISliderElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property()
	value: {
		to?: number;
		from?: number;
	} = {};

	@state()
	_enableRange?: boolean;

	@state()
	_initVal1?: number;

	@state()
	_initVal2?: number;

	@state()
	_step?: number;

	@state()
	_min?: number;

	@state()
	_max?: number;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		const enableRange = config.find((x) => x.alias === 'enableRange');
		if (enableRange) this._enableRange = enableRange.value as boolean;

		const initVal1 = config.find((x) => x.alias === 'initVal1');
		if (initVal1) this._initVal1 = initVal1.value as number;

		const initVal2 = config.find((x) => x.alias === 'initVal2');
		if (initVal2) this._initVal2 = initVal2.value as number;

		const step = config.find((x) => x.alias === 'step');
		if (step) this._step = step.value as number;

		const min = config.find((x) => x.alias === 'minVal');
		if (min) this._min = min.value as number;

		const max = config.find((x) => x.alias === 'maxVal');
		if (max) this._max = max.value as number;
	}

	#getValueObject(val: string) {
		if (!val.includes(',')) return { from: parseInt(val), to: parseInt(val) };
		const from = val.slice(0, val.indexOf(','));
		const to = val.slice(val.indexOf(',') + 1);
		return { from: parseInt(from), to: parseInt(to) };
	}

	private _onChange(event: CustomEvent) {
		const eventVal = this.#getValueObject((event.target as UmbInputSliderElement).value as string);
		this.value = eventVal;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-slider
			.initVal1="${this._initVal1 ?? 0}"
			.initVal2="${this._initVal2 ?? 0}"
			.step="${this._step ?? 0}"
			.min="${this._min ?? 0}"
			.max="${this._max ?? 0}"
			?enable-range=${this._enableRange}
			@change="${this._onChange}"></umb-input-slider>`;
	}
}

export default UmbPropertyEditorUISliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-slider': UmbPropertyEditorUISliderElement;
	}
}
