import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui-input';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from 'src/core/modal';

export type MultipleTextStringValue = Array<MultipleTextStringValueItem>;

export interface MultipleTextStringValueItem {
	value: string;
}

/**
 * @element umb-input-multiple-text-string
 */
@customElement('umb-input-multiple-text-string')
export class UmbInputMultipleTextStringElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			.item {
				display: flex;
				align-items: center;
				margin-bottom: var(--uui-size-space-3);
				gap: var(--uui-size-space-3);
			}

			.item #text-field {
				flex: 1;
			}

			#action {
				display: block;
			}
		`,
	];

	@state()
	private _value: MultipleTextStringValue = [];

	@property({ type: Array })
	public get value(): MultipleTextStringValue {
		return this._value;
	}
	public set value(value: MultipleTextStringValue) {
		this._value = value || [];
	}

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});
	}

	#onAdd() {
		this._value = [...this._value, { value: '' }];
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: false, cancelable: false }));
		this.#focusNewItem();
	}

	#onDelete(index: number) {
		const item = this._value[index];

		const modalHandler = this._modalService?.confirm({
			headline: `Delete ${item.value || 'item'}`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed) this.#deleteItem(index);
		});
	}

	#onInput(event: UUIInputEvent, currentIndex: number) {
		const target = event.currentTarget as UUIInputElement;
		const value = target.value as string;
		this._value = this._value.map((item, index) => (index === currentIndex ? { value } : item));
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: false, cancelable: false }));
	}

	async #focusNewItem() {
		await this.updateComplete;
		const inputs = this.shadowRoot?.querySelectorAll('uui-input') as NodeListOf<UUIInputElement>;
		const lastInput = inputs[inputs.length - 1];
		lastInput.focus();
	}

	#deleteItem(itemIndex: number) {
		this._value = this._value.filter((item, index) => index !== itemIndex);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: false, cancelable: false }));
	}

	render() {
		return html`
			${this._renderItems()}
			<uui-button id="action" label="Add" look="placeholder" color="default" @click="${this.#onAdd}"></uui-button>
		`;
	}

	private _renderItems() {
		return html`
			${repeat(
				this._value,
				(item, index) => index,
				(item, index) => html`${this._renderItem(item, index)}`
			)}
		`;
	}

	private _renderItem(item: MultipleTextStringValueItem, index: number) {
		return html`<div class="item">
			<uui-icon name="umb:navigation"></uui-icon>
			<uui-input
				id="text-field"
				value="${item.value}"
				@input="${(event: UUIInputEvent) => this.#onInput(event, index)}"></uui-input>
			<uui-button
				label="Delete ${item.value}"
				look="primary"
				color="danger"
				@click="${() => this.#onDelete(index)}"
				compact>
				<uui-icon name="umb:trash"></uui-icon>
			</uui-button>
		</div>`;
	}
}

export default UmbInputMultipleTextStringElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multiple-text-string': UmbInputMultipleTextStringElement;
	}
}
