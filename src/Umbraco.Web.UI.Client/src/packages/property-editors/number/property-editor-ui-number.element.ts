import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-number')
export class UmbPropertyEditorUINumberElement
	extends UmbFormControlMixin<number | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
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
		this._min = this.#parseInt(config.getValueByAlias('min')) || 0;
		this._max = this.#parseInt(config.getValueByAlias('max')) || Infinity;
		this._step = this.#parseInt(config.getValueByAlias('step'));
		this._placeholder = config.getValueByAlias('placeholder');
	}

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context.getLabel();
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
	}

	#parseInt(input: unknown): number | undefined {
		const num = Number(input);
		return Number.isNaN(num) ? undefined : num;
	}

	#onInput(e: InputEvent & { target: HTMLInputElement }) {
		this.value = this.#parseInt(e.target.value);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				type="number"
				label=${ifDefined(this._label)}
				min=${ifDefined(this._min)}
				max=${ifDefined(this._max)}
				step=${ifDefined(this._step)}
				placeholder=${ifDefined(this._placeholder)}
				value=${this.value?.toString() ?? (this._placeholder ? '' : '0')}
				@input=${this.#onInput}
				?readonly=${this.readonly}>
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
