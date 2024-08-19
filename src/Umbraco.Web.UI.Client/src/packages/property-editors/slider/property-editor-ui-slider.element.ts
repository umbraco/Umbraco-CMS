import type { UmbInputSliderElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

export type UmbSliderValue = { from: number; to: number } | undefined;

/**
 * @element umb-property-editor-ui-slider
 */
@customElement('umb-property-editor-ui-slider')
export class UmbPropertyEditorUISliderElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value: UmbSliderValue | undefined;

	@state()
	_enableRange = false;

	@state()
	_initVal1: number = 0;

	@state()
	_initVal2: number = 1;

	@state()
	_step = 1;

	@state()
	_min = 0;

	@state()
	_max = 100;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._enableRange = Boolean(config.getValueByAlias('enableRange'));

		// Make sure that step is higher than 0 (decimals ok).
		const step = (config.getValueByAlias('step') ?? 1) as number;
		this._step = step > 0 ? step : 1;

		const initVal1 = Number(config.getValueByAlias('initVal1'));
		this._initVal1 = isNaN(initVal1) ? 0 : initVal1;

		const initVal2 = Number(config.getValueByAlias('initVal2'));
		this._initVal2 = isNaN(initVal2) ? this._initVal1 + this._step : initVal2;

		const minVal = Number(config.getValueByAlias('minVal'));
		this._min = isNaN(minVal) ? 0 : minVal;

		const maxVal = Number(config.getValueByAlias('maxVal'));
		this._max = isNaN(maxVal) ? 100 : maxVal;

		if (this._min === this._max) {
			this._max = this._min + 100;
			//TODO Maybe we want to show some kind of error element rather than trying to fix the mistake made by the user...?
			throw new Error(
				`Property Editor Slider: min and max are currently equal. Please change your data type configuration. To render the slider correctly, we changed this slider to: min = ${this._min}, max = ${this._max}`,
			);
		}
	}

	#getValueObject(value: string) {
		const [from, to] = value.split(',').map(Number);
		return { from, to: to ?? from };
	}

	#onChange(event: CustomEvent & { target: UmbInputSliderElement }) {
		this.value = this.#getValueObject(event.target.value as string);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-slider
				.valueLow=${this.value?.from ?? this._initVal1}
				.valueHigh=${this.value?.to ?? this._initVal2}
				.step=${this._step}
				.min=${this._min}
				.max=${this._max}
				?enable-range=${this._enableRange}
				@change=${this.#onChange}>
			</umb-input-slider>
		`;
	}
}

export default UmbPropertyEditorUISliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-slider': UmbPropertyEditorUISliderElement;
	}
}
