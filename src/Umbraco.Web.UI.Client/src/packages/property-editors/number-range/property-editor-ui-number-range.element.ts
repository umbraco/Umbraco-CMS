import type { UmbInputNumberRangeElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-number-range
 */
@customElement('umb-property-editor-ui-number-range')
export class UmbPropertyEditorUINumberRangeElement
	extends UmbFormControlMixin<UmbNumberRangeValueType | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@state()
	_minValue?: number;

	@state()
	_maxValue?: number;

	@state()
	_validationRange?: UmbNumberRangeValueType;

	@property({ type: Object })
	public set value(value: UmbNumberRangeValueType | undefined) {
		this.#value = value || { min: undefined, max: undefined };
		this._minValue = value?.min;
		this._maxValue = value?.max;
	}
	public get value() {
		return this.#value;
	}
	#value: UmbNumberRangeValueType = { min: undefined, max: undefined };

	public set config(config: UmbPropertyEditorConfigCollection) {
		if (!config) return;
		this._validationRange = config.getValueByAlias<UmbNumberRangeValueType>('validationRange');
	}

	#onChange(event: CustomEvent & { target: UmbInputNumberRangeElement }) {
		this.value = { min: event.target.minValue, max: event.target.maxValue };
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-number-range')!);
	}

	focus(): void {
		this.shadowRoot!.querySelector('umb-input-number-range')!.focus();
	}

	render() {
		return html`
			<umb-input-number-range
				.minValue=${this._minValue}
				.maxValue=${this._maxValue}
				.validationRange=${this._validationRange}
				@change=${this.#onChange}>
			</umb-input-number-range>
		`;
	}
}

export default UmbPropertyEditorUINumberRangeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-number-range': UmbPropertyEditorUINumberRangeElement;
	}
}
