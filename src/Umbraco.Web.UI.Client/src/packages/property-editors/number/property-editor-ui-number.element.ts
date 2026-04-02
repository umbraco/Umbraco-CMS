import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-number')
export class UmbPropertyEditorUINumberElement
	extends UmbFormControlMixin<number, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	private _label?: string;

	@state()
	private _max?: number;

	@state()
	private _min?: number;

	@state()
	private _step?: number;

	@state()
	private _placeholder?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this._min = this.#parseNumber(config.getValueByAlias('min'));
		this._max = this.#parseNumber(config.getValueByAlias('max'));
		this._step = this.#parseNumber(config.getValueByAlias('step'));
		this._placeholder = this.localize.string(config.getValueByAlias<string>('placeholder') ?? '');
	}

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context?.getLabel();
		});

		this.addValidator(
			'rangeUnderflow',
			() => this.localize.term('validation_numberMinimum', this._min),
			() => !!this._min && this.value! < this._min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.localize.term('validation_numberMaximum', this._max),
			() => !!this._max && this.value! > this._max,
		);

		this.addValidator(
			'customError',
			() => this.localize.term('validation_numberMisconfigured', this._min, this._max),
			() => !!this._min && !!this._max && this._min > this._max,
		);
	}

	protected override firstUpdated() {
		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property '${this._label}' (Numeric) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-input')!);
	}

	#parseNumber(input: unknown): number | undefined {
		const num = Number(input);
		return Number.isFinite(num) ? num : undefined;
	}

	#onChange(event: InputEvent & { target: HTMLInputElement }) {
		const newValue = event.target.value === '' ? undefined : this.#parseNumber(event.target.value);
		if (newValue === this.value) return;
		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				type="number"
				label=${ifDefined(this._label)}
				min=${ifDefined(this._min)}
				max=${ifDefined(this._max)}
				step=${ifDefined(this._step)}
				value=${this.value?.toString() ?? ''}
				.placeholder=${this._placeholder ?? ''}
				.requiredMessage=${this.mandatoryMessage}
				?required=${this.mandatory}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</uui-input>
		`;
	}

	static override styles = [
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUINumberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-number': UmbPropertyEditorUINumberElement;
	}
}
