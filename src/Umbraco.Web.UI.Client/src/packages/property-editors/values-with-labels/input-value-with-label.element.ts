import { css, customElement, html, nothing, property, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UUIFormControlWithBasicsMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A single option of a value list: its value and its label.
 * @element umb-input-value-with-label
 */
@customElement('umb-input-value-with-label')
export class UmbInputValueWithLabelElement extends UUIFormControlWithBasicsMixin(UmbLitElement, '') {
	/**
	 * The label shown for the option.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	label = '';

	/**
	 * Makes the option readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Whether the option has just been added. The value of a new option follows its label until the value is edited.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, attribute: false })
	isNew = false;

	@query('uui-input')
	protected _firstInput?: UUIInputElement;

	#valueEdited = false;

	#onValueInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.#valueEdited = true;
		this.value = (event.currentTarget as UUIInputElement).value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// A field is only validated on leaving if its value changed; an empty field that was never typed in must be validated too.
	#onFieldFocusOut(event: FocusEvent) {
		(event.currentTarget as UUIInputElement).checkValidity();
	}

	public override checkValidity() {
		const inputs = Array.from(this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input') ?? []);
		return inputs.map((input) => input.checkValidity()).every(Boolean);
	}

	#onLabelInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = (event.currentTarget as UUIInputElement).value as string;
		if (this.isNew && !this.#valueEdited) this.value = generateAlias(this.label);
		this.dispatchEvent(new UmbChangeEvent());
	}

	// Pressing a button must not blur the field being edited, or an error it then shows can move the button before the click lands.
	#onButtonMouseDown(event: MouseEvent) {
		event.preventDefault();
	}

	#onDelete() {
		this.dispatchEvent(new UmbDeleteEvent());
	}

	// Keep the validity events inside the message element
	#onValid(event: Event) {
		event.stopPropagation();
	}

	#onInvalid(event: Event) {
		event.stopPropagation();
	}

	public override focus() {
		void this.#focusFirstInput();
	}

	async #focusFirstInput() {
		await this.updateComplete;
		await this._firstInput?.updateComplete;
		await this._firstInput?.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			${this.readonly ? nothing : html`<uui-icon name="icon-grip" class="handle"></uui-icon>`}
			<umb-form-validation-message @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<uui-input
					class="label"
					label=${this.localize.term('placeholders_label')}
					placeholder=${this.localize.term('placeholders_label')}
					.value=${this.label}
					?readonly=${this.readonly}
					required
					required-message=${this.localize.term('validation_invalidEmptyField', this.localize.term('general_label'))}
					@input=${this.#onLabelInput}
					@focusout=${this.#onFieldFocusOut}></uui-input>
			</umb-form-validation-message>
			<umb-form-validation-message @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<uui-input
					class="value"
					label=${this.localize.term('general_value')}
					placeholder=${this.localize.term('general_value')}
					.value=${this.value}
					?readonly=${this.readonly}
					@input=${this.#onValueInput}
					@focusout=${this.#onFieldFocusOut}></uui-input>
			</umb-form-validation-message>
			${this.readonly
				? nothing
				: html`<uui-button
						class="delete"
						compact
						look="outline"
						label=${this.localize.term('actions_delete')}
						@mousedown=${this.#onButtonMouseDown}
						@click=${this.#onDelete}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>`}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: grid;
				grid-column: 1 / -1;
				grid-template-columns: subgrid;
				align-items: start;
				margin-bottom: var(--uui-size-space-3);
			}

			.handle {
				cursor: grab;
				margin-top: var(--uui-size-space-3);
			}

			.handle:active {
				cursor: grabbing;
			}

			umb-form-validation-message {
				min-width: 0;
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbInputValueWithLabelElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-value-with-label': UmbInputValueWithLabelElement;
	}
}
