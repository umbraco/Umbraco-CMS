import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbDataTypePickerContext } from './data-type-input.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-data-type-input')
export class UmbDataTypeInputElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
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
	public set min(value: number | undefined) {
		if (value !== undefined) {
			this.#pickerContext.min = value;
		}
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
	 * @default undefined
	 */
	private _max: number | undefined;
	@property({ type: Number })
	public get max(): number | undefined {
		return this._max;
	}
	public set max(value: number | undefined) {
		if (value !== undefined) {
			this.#pickerContext.max = value;
		}
		this._max = value;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	private _selectedIds: Array<string> = [];
	public get selectedIds(): Array<string> {
		return this._selectedIds;
	}
	public set selectedIds(ids: Array<string>) {
		this._selectedIds = ids;
		super.value = ids.join(',');
	}

	@property()
	public set value(idsString: string) {
		if (idsString !== this._value) {
			this.selectedIds = idsString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<DataTypeItemResponseModel>;

	#pickerContext = new UmbDataTypePickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedIds.length < this.min
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedIds.length > this.max
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.selectedIds = selection));
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
}

export default UmbDataTypeInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-input': UmbDataTypeInputElement;
	}
}
