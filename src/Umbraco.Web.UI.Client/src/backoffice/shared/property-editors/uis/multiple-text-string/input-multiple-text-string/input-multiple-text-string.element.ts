import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui-input';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from 'src/core/modal';
import { UmbChangeEvent } from 'src/core/events/change.event';

export type MultipleTextStringValue = Array<MultipleTextStringValueItem>;

export interface MultipleTextStringValueItem {
	value: string;
}

/**
 * @element umb-input-multiple-text-string
 */
@customElement('umb-input-multiple-text-string')
export class UmbInputMultipleTextStringElement extends FormControlMixin(UmbLitElement) {
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

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._items.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._items.length > this.max
		);

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});
	}

	@state()
	private _items: MultipleTextStringValue = [];

	@property({ type: Array })
	public get items(): MultipleTextStringValue {
		return this._items;
	}
	public set items(items: MultipleTextStringValue) {
		this._items = items || [];
	}

	@property()
	public set value(itemsString: string) {
		// TODO: implement value setter and getter
		throw new Error('Not implemented');
	}

	#onAdd() {
		this._items = [...this._items, { value: '' }];
		this.dispatchEvent(new UmbChangeEvent());
		this.#focusNewItem();
	}

	#onDelete(index: number) {
		const item = this._items[index];

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
		this._items = this._items.map((item, index) => (index === currentIndex ? { value } : item));
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: false, cancelable: false }));
	}

	async #focusNewItem() {
		await this.updateComplete;
		const inputs = this.shadowRoot?.querySelectorAll('uui-input') as NodeListOf<UUIInputElement>;
		const lastInput = inputs[inputs.length - 1];
		lastInput.focus();
	}

	#deleteItem(itemIndex: number) {
		this._items = this._items.filter((item, index) => index !== itemIndex);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: false, cancelable: false }));
	}

	protected getFormElement() {
		return undefined;
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
				this._items,
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
