import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

/**
 *
 * @param value
 */
function getNumberOrUndefined(value: string) {
	const num = parseInt(value, 10);
	return isNaN(num) ? undefined : num;
}

@customElement('umb-input-number-range')
export class UmbInputNumberRangeElement extends UmbFormControlMixin(UmbLitElement) {
	@property({ type: String, attribute: 'min-label' })
	minLabel = 'Low value';

	@property({ type: String, attribute: 'max-label' })
	maxLabel = 'High value';

	@state()
	private _minValue?: number;

	@property({ type: Number })
	public set minValue(value: number | undefined) {
		this._minValue = value;
		this.updateValue();
	}
	public get minValue() {
		return this._minValue;
	}

	@state()
	private _maxValue?: number;

	@property({ type: Number })
	public set maxValue(value: number | undefined) {
		this._maxValue = value;
		this.updateValue();
	}
	public get maxValue() {
		return this._maxValue;
	}

	@property({ type: Object })
	validationRange?: UmbNumberRangeValueType;

	private updateValue() {
		const newValue =
			this._minValue || this._maxValue ? (this._minValue ?? '') + ',' + (this._maxValue ?? '') : undefined;
		if (super.value !== newValue) {
			super.value = newValue;
		}
	}

	@property()
	public override set value(valueString: string | undefined) {
		if (valueString !== this.value) {
			if (valueString === undefined) {
				this.minValue = this.maxValue = undefined;
				return;
			}
			const splittedValue = valueString.split(/[ ,]+/);
			this.minValue = getNumberOrUndefined(splittedValue[0]);
			this.maxValue = getNumberOrUndefined(splittedValue[1]);
		}
	}
	public override get value(): string | undefined {
		return this.minValue || this.maxValue ? (this.minValue || '') + ',' + (this.maxValue || '') : undefined;
	}

	constructor() {
		super();

		this.addValidator(
			'patternMismatch',
			() => {
				return 'The low value must not be exceed the high value';
			},
			() => {
				return this._minValue !== undefined && this._maxValue !== undefined ? this._minValue > this._maxValue : false;
			},
		);
	}

	override firstUpdated() {
		this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input').forEach((x) => this.addFormControlElement(x));
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#onMinInput(e: InputEvent & { target: HTMLInputElement }) {
		const value = e.target.value;
		this.minValue = value ? Number(value) : undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onMaxInput(e: InputEvent & { target: HTMLInputElement }) {
		const value = e.target.value;
		this.maxValue = value ? Number(value) : undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				type="number"
				label=${this.minLabel}
				min=${ifDefined(this.validationRange?.min)}
				max=${ifDefined(this.validationRange?.max)}
				placeholder=${this.validationRange?.min ?? ''}
				.value=${this._minValue}
				@input=${this.#onMinInput}></uui-input>
			<b>–</b>
			<uui-input
				type="number"
				label=${this.maxLabel}
				min=${ifDefined(this.validationRange?.min)}
				max=${ifDefined(this.validationRange?.max)}
				placeholder=${this.validationRange?.max ?? '∞'}
				.value=${this._maxValue}
				@input=${this.#onMaxInput}></uui-input>
		`;
	}

	static override styles = css`
		:host(:invalid:not([pristine])) {
			color: var(--uui-color-danger);
		}
		:host(:invalid:not([pristine])) uui-input {
			border-color: var(--uui-color-danger);
		}
	`;
}

export default UmbInputNumberRangeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-number-range': UmbInputNumberRangeElement;
	}
}
