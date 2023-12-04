import { css, html, nothing, customElement, property, query, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import {
	FormControlMixin,
	UUIColorPickerElement,
	UUIInputElement,
	UUIInputEvent,
} from '@umbraco-cms/backoffice/external/uui';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent, UmbInputEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-multiple-color-picker-item-input
 */
@customElement('umb-multiple-color-picker-item-input')
export class UmbMultipleColorPickerItemInputElement extends FormControlMixin(UmbLitElement) {
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: String })
	label?: string;

	@query('#input')
	protected _input?: UUIInputElement;

	@query('#color')
	protected _colorPicker!: UUIColorPickerElement;

	private _modalContext?: UmbModalManagerContext;

	@property({ type: Boolean })
	showLabels = true;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#onDelete() {
		const modalContext = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			headline: `${this.localize.term('actions_delete')} ${this.value || ''}`,
			content: this.localize.term('content_nestedContentDeleteItem'),
			color: 'danger',
			confirmLabel: this.localize.term('actions_delete'),
		});

		modalContext?.onSubmit().then(() => {
			this.dispatchEvent(new UmbDeleteEvent());
		});
	}

	#onLabelInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = event.target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onLabelChange(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = event.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onValueChange(event: UUIInputEvent) {
		event.stopPropagation();
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onValueInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.value = event.target.value;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onColorInput(event: InputEvent) {
		event.stopPropagation();
		this.value = this._colorPicker.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// Prevent valid events from bubbling outside the message element
	#onValid(event: any) {
		event.stopPropagation();
	}

	// Prevent invalid events from bubbling outside the message element
	#onInvalid(event: any) {
		event.stopPropagation();
	}

	public async focus() {
		await this.updateComplete;
		this._input?.focus();
	}

	protected getFormElement() {
		return undefined;
	}

	#onColorClick() {
		this._colorPicker.click();
	}

	render() {
		//TODO: Using native input=color element instead of uui-color-picker due to its huge size and bad adaptability as a pop up
		return html`
			<uui-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<div id="item">
					${this.disabled || this.readonly ? nothing : html`<uui-icon name="icon-navigation"></uui-icon>`}
					<div class="color-wrapper">
						<uui-input
							id="input"
							value=${this.value}
							label=${this.localize.term('general_value')}
							placeholder=${this.localize.term('general_value')}
							@input="${this.#onValueInput}"
							@change="${this.#onValueChange}"
							required="${this.required}"
							required-message="Value is missing">
							<uui-color-swatch
								slot="prepend"
								label=${this.value}
								value="${this.value}"
								@click=${this.#onColorClick}></uui-color-swatch>
						</uui-input>
						<input aria-hidden="${true}" type="color" id="color" value=${this.value} @input=${this.#onColorInput} />
					</div>
					${this.showLabels
						? html` <uui-input
								label=${this.localize.term('placeholders_label')}
								placeholder=${this.localize.term('placeholders_label')}
								value=${ifDefined(this.label)}
								@input="${this.#onLabelInput}"
								@change="${this.#onLabelChange}"
								?disabled=${this.disabled}
								?readonly=${this.readonly}></uui-input>`
						: nothing}
					${this.readonly
						? nothing
						: html`<uui-button
								label="${this.localize.term('actions_delete')} ${this.value}"
								look="primary"
								color="danger"
								@click="${this.#onDelete}"
								?disabled=${this.disabled}
								compact>
								<uui-icon name="icon-trash"></uui-icon>
						  </uui-button>`}
				</div>
			</uui-form-validation-message>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				margin-bottom: var(--uui-size-space-3);
				gap: var(--uui-size-space-3);
			}

			#item {
				position: relative;
				display: flex;
				gap: var(--uui-size-1);
				align-items: center;
			}
			uui-input {
				flex: 1;
			}

			uui-color-swatch {
				padding: var(--uui-size-1);
			}

			.color-wrapper {
				position: relative;
				flex: 1;
				display: flex;
				flex-direction: column;
			}

			uui-input,
			#validation-message {
				flex: 1;
			}

			input[type='color'] {
				visibility: hidden;
				width: 0px;
				padding: 0;
				margin: 0;
				position: absolute;
			}
		`,
	];
}

export default UmbMultipleColorPickerItemInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-color-picker-item-input': UmbMultipleColorPickerItemInputElement;
	}
}
