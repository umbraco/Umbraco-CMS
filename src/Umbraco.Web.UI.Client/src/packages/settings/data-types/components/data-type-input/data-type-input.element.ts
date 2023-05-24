import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbDataTypePickerContext } from './data-type-input.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-data-type-input')
export class UmbDataTypeInputElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public get min(): number {
		return this.#pickerContext.min;
	}
	public set min(value: number) {
		this.#pickerContext.min = value;
	}

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
	 * @default Infinity
	 */
	@property({ type: Number })
	public get max(): number {
		return this.#pickerContext.max;
	}
	public set max(value: number) {
		this.#pickerContext.max = value;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public get selectedIds(): Array<string> {
		return this.#pickerContext.getSelection();
	}
	public set selectedIds(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = idsString.split(/[ ,]+/);
	}

	@property()
	get pickableFilter() {
		return this.#pickerContext.pickableFilter;
	}
	set pickableFilter(newVal) {
		this.#pickerContext.pickableFilter = newVal;
	}

	@state()
	private _items?: Array<DataTypeItemResponseModel>;

	#pickerContext = new UmbDataTypePickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max
		);

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`
			<uui-ref-list>${this._items?.map((item) => this._renderItem(item))}</uui-ref-list>
			<uui-button id="add-button" look="placeholder" @click=${() => this.#pickerContext.openPicker()} label="open"
				>Add</uui-button
			>
		`;
	}

	private _renderItem(item: DataTypeItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node-data-type name=${item.name}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove Data Type ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node-data-type>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbDataTypeInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-input': UmbDataTypeInputElement;
	}
}
