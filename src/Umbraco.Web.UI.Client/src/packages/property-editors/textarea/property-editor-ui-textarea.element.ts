import { css, customElement, html, ifDefined, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { StyleInfo } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement
	extends UmbFormControlMixin<string, typeof UmbLitElement>(UmbLitElement, undefined)
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
	private _maxChars?: number;

	@state()
	private _rows?: number;

	@state()
	private _maxHeight?: number;

	@state()
	private _minHeight?: number;

	@state()
	private _css: StyleInfo = {};

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._maxChars = Number(config?.getValueByAlias('maxChars')) || undefined;
		this._rows = Number(config?.getValueByAlias('rows')) || undefined;
		this._minHeight = Number(config?.getValueByAlias('minHeight')) || undefined;
		this._maxHeight = Number(config?.getValueByAlias('maxHeight')) || undefined;

		this._css = {
			'--uui-textarea-min-height': this._minHeight ? `${this._minHeight}px` : 'reset',
			'--uui-textarea-max-height': this._maxHeight ? `${this._maxHeight}px` : 'reset',
		};
	}

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context.getLabel();
		});
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-textarea')!);

		if (this._minHeight && this._maxHeight && this._minHeight > this._maxHeight) {
			console.warn(
				`Property '${this._label}' (Textarea) has been misconfigured, 'minHeight' is greater than 'maxHeight'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onInput(event: InputEvent) {
		const newValue = (event.target as HTMLTextAreaElement).value;
		if (newValue === this.value) return;
		this.value = newValue;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<uui-textarea
				label=${ifDefined(this._label)}
				style=${styleMap(this._css)}
				.autoHeight=${this._rows ? false : true}
				maxlength=${ifDefined(this._maxChars)}
				rows=${ifDefined(this._rows)}
				.value=${this.value ?? ''}
				@input=${this.#onInput}
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
