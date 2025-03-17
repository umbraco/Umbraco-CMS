import { css, customElement, html, ifDefined, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { StyleInfo } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement
	extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
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

	@state()
	private _maxChars?: number;

	@state()
	private _rows?: number;

	@state()
	private _css: StyleInfo = {};

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._maxChars = Number(config?.getValueByAlias('maxChars')) || undefined;
		this._rows = Number(config?.getValueByAlias('rows')) || undefined;
		// min/max height where for a short period present in the config, but we do not want this complexity of our configuration.
		// @deprecated remove config option in v.18, leave good default.
		const _minHeight = Number(config?.getValueByAlias('minHeight')) || undefined;
		const _maxHeight = Number(config?.getValueByAlias('maxHeight')) || undefined;

		this._css = {
			'--uui-textarea-min-height': _minHeight ? `${_minHeight}px` : 'reset',
			'--uui-textarea-max-height': _maxHeight ? `${_maxHeight}px` : '33vh',
		};
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-textarea')!);
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUITextareaElement>('uui-textarea')?.focus();
	}

	#onInput(event: InputEvent) {
		const newValue = (event.target as HTMLTextAreaElement).value;
		if (newValue === this.value) return;
		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-textarea
				.label=${this.localize.term('general_fieldFor', [this.name])}
				style=${styleMap(this._css)}
				.autoHeight=${this._rows ? false : true}
				maxlength=${ifDefined(this._maxChars)}
				rows=${ifDefined(this._rows)}
				.value=${this.value ?? ''}
				@input=${this.#onInput}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?readonly=${this.readonly}></uui-textarea>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-textarea {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUITextareaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-textarea': UmbPropertyEditorUITextareaElement;
	}
}
