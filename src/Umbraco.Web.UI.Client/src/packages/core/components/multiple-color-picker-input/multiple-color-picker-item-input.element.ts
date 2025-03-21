import {
	css,
	html,
	nothing,
	customElement,
	property,
	query,
	ifDefined,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent, UmbInputEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIColorPickerElement, UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-multiple-color-picker-item-input
 */
@customElement('umb-multiple-color-picker-item-input')
export class UmbMultipleColorPickerItemInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ type: String })
	public override set value(value: string) {
		if (value.startsWith('#')) {
			this._valueHex = value;
			super.value = value.substring(1);
		} else {
			super.value = value;
			this._valueHex = `#${value}`;
		}
	}
	public override get value() {
		return super.value as string;
	}

	@state()
	private _valueHex = '';

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

	@property({ type: Boolean })
	showLabels = false;

	async #onDelete() {
		await umbConfirmModal(this, {
			headline: `${this.localize.term('actions_delete')} ${this.value || ''}`,
			content: this.localize.term('content_nestedContentDeleteItem'),
			color: 'danger',
			confirmLabel: this.localize.term('actions_delete'),
		});

		this.dispatchEvent(new UmbDeleteEvent());
	}

	#onLabelInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = event.target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onLabelKeydown(event: KeyboardEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		if (event.key === 'Enter' && target.value) {
			this.dispatchEvent(new CustomEvent('enter'));
		}
	}

	#onLabelChange(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = event.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onValueKeydown(event: KeyboardEvent) {
		event.stopPropagation();
		if (event.key === 'Enter') this.#onColorClick();
	}

	#onValueChange(event: UUIInputEvent) {
		event.stopPropagation();
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onValueInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onColorChange(event: Event) {
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

	public override async focus() {
		await this.updateComplete;
		this._input?.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	#onColorClick() {
		this._colorPicker.click();
	}

	override render() {
		//TODO: Using native input=color element instead of uui-color-picker due to its huge size and bad adaptability as a pop up
		return html`
			<umb-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<div id="item">
					${this.disabled || this.readonly ? nothing : html`<uui-icon name="icon-grip"></uui-icon>`}
					<div class="color-wrapper">
						<uui-input
							id="input"
							value=${this.value}
							label=${this.localize.term('general_value')}
							placeholder=${this.localize.term('general_value')}
							required=${this.required}
							required-message="Value is missing"
							@keydown=${this.#onValueKeydown}
							@input=${this.#onValueInput}
							@change=${this.#onValueChange}>
							<uui-color-swatch
								slot="prepend"
								label=${this.value}
								value=${this._valueHex}
								@click=${this.#onColorClick}></uui-color-swatch>
						</uui-input>
						<input aria-hidden="${true}" type="color" id="color" value=${this.value} @change=${this.#onColorChange} />
					</div>
					${when(
						this.showLabels,
						() => html`
							<uui-input
								label=${this.localize.term('placeholders_label')}
								placeholder=${this.localize.term('placeholders_label')}
								value=${ifDefined(this.label)}
								@keydown=${this.#onLabelKeydown}
								@input="${this.#onLabelInput}"
								@change="${this.#onLabelChange}"
								?disabled=${this.disabled}
								?readonly=${this.readonly}></uui-input>
						`,
					)}
					${when(
						!this.readonly,
						() => html`
							<uui-button
								compact
								color="danger"
								label=${this.localize.term('actions_delete')}
								look="primary"
								?disabled=${this.disabled}
								@click=${this.#onDelete}>
								<uui-icon name="icon-trash"></uui-icon>
							</uui-button>
						`,
					)}
				</div>
			</umb-form-validation-message>
		`;
	}

	static override styles = [
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

			uui-color-swatch:focus-within {
				outline: 2px solid var(--uui-color-selected);
				outline-offset: 0;
				border-radius: var(--uui-border-radius);
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
