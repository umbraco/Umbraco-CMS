import { css, customElement, html, nothing, property, query } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlWithBasicsMixin } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY } from '@umbraco-cms/backoffice/validation';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A single option of a value list: its value and its label.
 * @element umb-input-value-list-option
 */
@customElement('umb-input-value-list-option')
export class UmbInputValueListOptionElement extends UUIFormControlWithBasicsMixin(UmbLitElement, '') {
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

	@query('uui-input')
	protected _firstInput?: UUIInputElement;

	// A new option starts without a value, and only a new option takes its value from its label.
	#isNew = false;

	// The label as it was before the last edit.
	#previousLabel = '';

	protected override willUpdate(changedProperties: PropertyValues) {
		super.willUpdate(changedProperties);
		if (!this.hasUpdated) this.#isNew = this.value === '';
	}

	#onValueInput(event: UUIInputEvent) {
		event.stopPropagation();
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
		if (this.#isNew && (this.value === '' || this.value === this.#previousLabel)) this.value = this.label;
		this.#previousLabel = this.label;
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
					required-message=${UMB_VALIDATION_EMPTY_LOCALIZATION_KEY}
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
					required
					required-message=${UMB_VALIDATION_EMPTY_LOCALIZATION_KEY}
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

	static override styles = [
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

export default UmbInputValueListOptionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-value-list-option': UmbInputValueListOptionElement;
	}
}
