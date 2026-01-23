import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	InputMode as UUIInputMode,
	InputType as UUIInputType,
	UUIInputElement,
} from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
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

	/**
	 * The name of this field.
	 * @type {string}
	 */
	@property({ type: String })
	name?: string;

	#defaultType: UUIInputType = 'text';

	#defaultInputMode: UUIInputMode = 'text';

	@state()
	private _type: UUIInputType = this.#defaultType;

	@state()
	private _inputMode: UUIInputMode = this.#defaultInputMode;

	@state()
	private _maxChars?: number;

	@state()
	private _placeholder?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._type = config.getValueByAlias<UUIInputType>('inputType') ?? this.#defaultType;
		this._inputMode = config.getValueByAlias<UUIInputMode>('inputMode') || this.#defaultInputMode;
		this._maxChars = this.#parseNumber(config.getValueByAlias('maxChars'));
		this._placeholder = this.localize.string(config.getValueByAlias<string>('placeholder') ?? '');
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-input')!);
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#parseNumber(input: unknown): number | undefined {
		const num = Number(input);
		return Number.isFinite(num) ? num : undefined;
	}

	#getMaxLengthMessage(max: number, current: number) {
		const exceeded = current - max;
		return this.localize.term('textbox_characters_exceed', max, exceeded);
	}

	#onInput(e: InputEvent) {
		const newValue = (e.target as HTMLInputElement).value;
		if (newValue === this.value) return;
		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-input
				.inputMode=${this._inputMode}
				.label=${this.localize.term('general_fieldFor', [this.name])}
				.maxlength=${this._maxChars}
				.maxlengthMessage=${this.#getMaxLengthMessage.bind(this)}
				.placeholder=${this._placeholder ?? ''}
				.requiredMessage=${this.mandatoryMessage}
				.type=${this._type}
				.value=${this.value ?? ''}
				?readonly=${this.readonly}
				?required=${this.mandatory}
				@input=${this.#onInput}>
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

export default UmbPropertyEditorUITextBoxElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-text-box': UmbPropertyEditorUITextBoxElement;
	}
}
