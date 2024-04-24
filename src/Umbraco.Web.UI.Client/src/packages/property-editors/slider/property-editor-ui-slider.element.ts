import type { UmbInputSliderElement } from '../../core/components/input-slider/input-slider.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-slider
 */
@customElement('umb-property-editor-ui-slider')
export class UmbPropertyEditorUISliderElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value: { to?: number; from?: number } | undefined = undefined;

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

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._enableRange = config?.getValueByAlias('enableRange');
		this._initVal1 = config?.getValueByAlias('initVal1');
		this._initVal2 = config?.getValueByAlias('initVal2');
		this._step = config?.getValueByAlias('step');
		this._min = config?.getValueByAlias('minVal');
		this._max = config?.getValueByAlias('maxVal');
	}

	#getValueObject(value: string) {
		const [from, to] = value.split(',').map(Number);
		return { from, to: to ?? from };
	}

	#onChange(event: CustomEvent) {
		const element = event.target as UmbInputSliderElement;
		this.value = this.#getValueObject(element.value as string);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-slider
			.valueLow=${this.value?.from ?? this._initVal1 ?? 0}
			.valueHigh=${this.value?.to ?? this._initVal2 ?? 0}
			.step=${this._step ?? 0}
			.min=${this._min ?? 0}
			.max=${this._max ?? 0}
			?enable-range=${this._enableRange}
			@change=${this.#onChange}></umb-input-slider>`;
	}
}

export default UmbPropertyEditorUISliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-slider': UmbPropertyEditorUISliderElement;
	}
}
