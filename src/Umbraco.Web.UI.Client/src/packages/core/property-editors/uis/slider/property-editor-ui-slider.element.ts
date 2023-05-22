import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import UmbInputSliderElement from '../../../components/input-slider/input-slider.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-slider
 */
@customElement('umb-property-editor-ui-slider')
export class UmbPropertyEditorUISliderElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
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
	public set config(config: UmbDataTypePropertyCollection) {
		this._enableRange = config.getValueByAlias('enableRange');
		this._initVal1 = config.getValueByAlias('initVal1');
		this._initVal2 = config.getValueByAlias('initVal2');
		this._step = config.getValueByAlias('step');
		this._min = config.getValueByAlias('minVal');
		this._max = config.getValueByAlias('maxVal');
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

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUISliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-slider': UmbPropertyEditorUISliderElement;
	}
}
