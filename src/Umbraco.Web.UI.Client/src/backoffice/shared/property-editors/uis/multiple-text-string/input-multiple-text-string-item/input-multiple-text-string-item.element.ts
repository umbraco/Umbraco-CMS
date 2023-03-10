import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIInputEvent } from '@umbraco-ui/uui-input';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbChangeEvent, UmbInputEvent, UmbDeleteEvent } from '@umbraco-cms/events';
import { UmbLitElement } from '@umbraco-cms/element';
import { UMB_CONFIRM_MODAL_TOKEN } from 'src/backoffice/shared/modals/confirm';

/**
 * @element umb-input-multiple-text-string-item
 */
@customElement('umb-input-multiple-text-string-item')
export class UmbInputMultipleTextStringItemElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
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
		`,
	];

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

	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#onDelete() {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL_TOKEN, {
			headline: `Delete ${this.value || 'item'}`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		modalHandler?.onSubmit().then(() => {
			this.dispatchEvent(new UmbDeleteEvent());
		});
	}

	#onInput(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this.value = target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onChange(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this.value = target.value as string;
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

	render() {
		return html`
			${this.disabled || this.readonly ? nothing : html`<uui-icon name="umb:navigation"></uui-icon>`}
			<uui-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<uui-input
					id="input"
					label="Value"
					value="${this.value}"
					@input="${this.#onInput}"
					@change="${this.#onChange}"
					?disabled=${this.disabled}
					?readonly=${this.readonly}
					required="${this.required}"
					required-message="Value is missing"></uui-input>
			</uui-form-validation-message>

			${this.readonly
				? nothing
				: html`<uui-button
						label="Delete ${this.value}"
						look="primary"
						color="danger"
						@click="${this.#onDelete}"
						?disabled=${this.disabled}
						compact>
						<uui-icon name="umb:trash"></uui-icon>
				  </uui-button>`}
		`;
	}
}

export default UmbInputMultipleTextStringItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multiple-text-string-item': UmbInputMultipleTextStringItemElement;
	}
}
