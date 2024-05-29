import type { UmbInputSliderElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
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
	_enableRange = false;

	@state()
	_initVal1?: number;

	@state()
	_initVal2?: number;

	@state()
	_step = 1;

	@state()
	_min = 0;

	@state()
	_max = 100;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._enableRange = Boolean(config.getValueByAlias('enableRange')) ?? false;
		this._initVal1 = Number(config.getValueByAlias('initVal1'));
		this._initVal2 = Number(config.getValueByAlias('initVal2'));
		this._step = Number(config.getValueByAlias('step')) ?? 1;
		this._min = Number(config.getValueByAlias('minVal')) ?? 0;
		this._max = Number(config.getValueByAlias('maxVal')) ?? 100;
	}

	#getValueObject(value: string) {
		const [from, to] = value.split(',').map(Number);
		return { from, to: to ?? from };
	}

	#onChange(event: CustomEvent & { target: UmbInputSliderElement }) {
		this.value = this.#getValueObject(event.target.value as string);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-slider
				.valueLow=${this.value?.from ?? this._initVal1 ?? 0}
				.valueHigh=${this.value?.to ?? this._initVal2 ?? 0}
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
