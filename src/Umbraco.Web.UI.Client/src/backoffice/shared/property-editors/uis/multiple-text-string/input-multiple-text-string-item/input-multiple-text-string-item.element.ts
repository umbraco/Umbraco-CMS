import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIInputEvent } from '@umbraco-ui/uui-input';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from 'src/core/modal';
import { UmbChangeEvent } from 'src/core/events/change.event';
import { UmbDeleteEvent } from 'src/core/events/delete.event';
import { UmbInputEvent } from 'src/core/events/input.event';
import { elementUpdated } from '@open-wc/testing';

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

			#input {
				flex: 1;
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

	/**
	 * Makes the input required
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	required = false;

	@query('#input')
	protected _input?: UUIInputElement;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});
	}

	#onDelete() {
		const modalHandler = this._modalService?.confirm({
			headline: `Delete ${this._value || 'item'}`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed) this.dispatchEvent(new UmbDeleteEvent());
		});
	}

	#onInput(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this._value = target.value as string;
		this.dispatchEvent(new UmbInputEvent());
	}

	#onChange(event: UUIInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIInputElement;
		this._value = target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
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
			<uui-input
				id="input"
				value="${this._value}"
				@input="${this.#onInput}"
				@change="${this.#onChange}"
				?disabled=${this.disabled}
				?readonly=${this.readonly}></uui-input>

			${this.readonly
				? nothing
				: html`<uui-button
						label="Delete ${this._value}"
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
