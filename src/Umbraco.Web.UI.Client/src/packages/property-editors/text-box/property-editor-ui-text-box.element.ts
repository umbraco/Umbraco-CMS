import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

type UuiInputTypeType = typeof UUIInputElement.prototype.type;

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

	#defaultType: UuiInputTypeType = 'text';

	@state()
	private _type: UuiInputTypeType = this.#defaultType;

	@state()
	private _inputMode?: string;

	@state()
	private _maxChars?: number;

	@state()
	private _placeholder?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._type = config?.getValueByAlias<UuiInputTypeType>('inputType') ?? this.#defaultType;
		this._inputMode = config?.getValueByAlias('inputMode');
		this._maxChars = config?.getValueByAlias('maxChars');
		this._placeholder = config?.getValueByAlias('placeholder');
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-input')!);
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#onInput(e: InputEvent) {
		const newValue = (e.target as HTMLInputElement).value;
		if (newValue === this.value) return;
		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<uui-input
			.label=${this.localize.term('general_fieldFor', [this.name])}
			.value=${this.value ?? ''}
			.type=${this._type}
			placeholder=${ifDefined(this._placeholder)}
			inputMode=${ifDefined(this._inputMode)}
			maxlength=${ifDefined(this._maxChars)}
			@input=${this.#onInput}
			?required=${this.mandatory}
			.requiredMessage=${this.mandatoryMessage}
			?readonly=${this.readonly}></uui-input>`;
	}

	static override styles = [
		UmbTextStyles,
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
