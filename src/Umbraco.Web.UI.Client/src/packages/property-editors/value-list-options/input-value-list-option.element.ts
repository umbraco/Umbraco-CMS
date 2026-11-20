import { css, customElement, html, nothing, property, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlWithBasicsMixin } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY } from '@umbraco-cms/backoffice/validation';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A single option of a value list: its value and, optionally, its label.
 * @element umb-input-value-list-option
 */
@customElement('umb-input-value-list-option')
export class UmbInputValueListOptionElement extends UUIFormControlWithBasicsMixin(UmbLitElement, '') {
	/**
	 * The label shown for the option. Leave empty to show the value.
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

	@query('#value')
	protected _valueInput?: UUIInputElement;

	#onValueInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.value = (event.currentTarget as UUIInputElement).value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// A field is only validated on leaving if its value changed; an empty value that was never typed in must be validated too.
	#onValueFocusOut(event: FocusEvent) {
		(event.currentTarget as UUIInputElement).checkValidity();
	}

	#onLabelInput(event: UUIInputEvent) {
		event.stopPropagation();
		this.label = (event.currentTarget as UUIInputElement).value as string;
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

	public override async focus() {
		await this.updateComplete;
		await this._valueInput?.updateComplete;
		this._valueInput?.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			${this.readonly ? nothing : html`<uui-icon name="icon-grip" class="handle"></uui-icon>`}
			<umb-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<div id="fields">
					<uui-input
						id="value"
						class="value"
						label=${this.localize.term('general_value')}
						placeholder=${this.localize.term('general_value')}
						.value=${this.value}
						?readonly=${this.readonly}
						required
						required-message=${UMB_VALIDATION_EMPTY_LOCALIZATION_KEY}
						@input=${this.#onValueInput}
						@focusout=${this.#onValueFocusOut}></uui-input>
					<uui-input
						class="label"
						label=${this.localize.term('placeholders_label')}
						placeholder=${this.localize.term('placeholders_label')}
						.value=${this.label}
						?readonly=${this.readonly}
						@input=${this.#onLabelInput}></uui-input>
				</div>
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

			#validation-message {
				grid-column: span 2;
				min-width: 0;
			}

			#fields {
				display: flex;
				gap: var(--uui-size-space-3);
			}

			#fields uui-input {
				flex: 1;
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
