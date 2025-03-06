import { css, customElement, html, nothing, property, query, when } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent, UmbInputEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-input-multiple-text-string-item
 */
@customElement('umb-input-multiple-text-string-item')
export class UmbInputMultipleTextStringItemElement extends UUIFormControlMixin(UmbLitElement, '') {
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

	@query('#input')
	protected _input?: UUIInputElement;

	async #onDelete() {
		await umbConfirmModal(this, {
			headline: `Delete ${this.value || 'item'}`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		this.dispatchEvent(new UmbDeleteEvent());
	}

	#onInput(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this.value = target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onKeydown(event: KeyboardEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		if (event.key === 'Enter' && target.value) {
			this.dispatchEvent(new CustomEvent('enter'));
		}
	}

	#onChange(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this.value = target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// Prevent valid events from bubbling outside the message element
	#onValid(event: Event) {
		event.stopPropagation();
	}

	// Prevent invalid events from bubbling outside the message element
	#onInvalid(event: Event) {
		event.stopPropagation();
	}

	public override async focus() {
		await this.updateComplete;
		this._input?.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			${this.disabled || this.readonly ? nothing : html`<uui-icon name="icon-navigation" class="handle"></uui-icon>`}

			<umb-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<uui-input
					id="input"
					label="Value"
					value=${this.value}
					@keydown=${this.#onKeydown}
					@input=${this.#onInput}
					@change=${this.#onChange}
					?disabled=${this.disabled}
					?readonly=${this.readonly}
					required=${this.required}
					required-message="Value is missing"></uui-input>
			</umb-form-validation-message>

			${when(
				!this.readonly,
				() => html`
					<uui-button
						compact
						label="${this.localize.term('general_remove')} ${this.value}"
						look="outline"
						?disabled=${this.disabled}
						@click=${this.#onDelete}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				`,
			)}
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

			#validation-message {
				flex: 1;
			}

			#input {
				width: 100%;
			}

			.handle {
				cursor: move;
			}
		`,
	];
}

export default UmbInputMultipleTextStringItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multiple-text-string-item': UmbInputMultipleTextStringItemElement;
	}
}
