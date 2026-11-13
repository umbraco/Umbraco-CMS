import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import { UmbDataTypePickerInputContext } from './data-type-input.context.js';
import { css, html, customElement, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UUIFormControlWithBasicsMixin } from '@umbraco-cms/backoffice/external/uui';
import { isAboveZeroAndBelowMinimum } from '@umbraco-cms/backoffice/validation';

// TODO: Rename to 'umb-input-data-type'. [LK]
@customElement('umb-data-type-input')
export class UmbDataTypeInputElement extends UUIFormControlWithBasicsMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputDataType',
		itemSelector: 'uui-ref-node-data-type',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

	/**
	 * Min validation message. When not set, a localized message stating the minimum and how many more items are needed is used.
	 * @type {string}
	 * @attr
	 * @default undefined
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage?: string;

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set max(value: number) {
		this.#pickerContext.max = value;
		if (value === 1) {
			this.#sorter.disable();
		} else {
			this.#sorter.enable();
		}
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message. When not set, a localized message stating the maximum and how many items are over it is used.
	 * @type {string}
	 * @attr
	 * @default undefined
	 */
	@property({ type: String, attribute: 'max-message' })
	maxMessage?: string;

	@property({ type: Array })
	public set selection(uniques: Array<string>) {
		this.#pickerContext.setSelection(uniques ?? []);
		this.#sorter.setModel(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(uniques: string) {
		this.selection = splitStringToArray(uniques);
	}
	public override get value(): string {
		return this.selection.join(',');
	}

	@state()
	private _items?: Array<UmbDataTypeItemModel>;

	#pickerContext = new UmbDataTypePickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() =>
				this.minMessage ||
				this.localize.term(
					'validation_entriesShort',
					this.min,
					(this.min ?? 0) - this.#pickerContext.getSelection().length,
				),
			() => isAboveZeroAndBelowMinimum(this.#pickerContext.getSelection().length, this.min),
		);

		this.addValidator(
			'rangeOverflow',
			() =>
				this.maxMessage ||
				this.localize.term(
					'validation_entriesExceed',
					this.max,
					this.#pickerContext.getSelection().length - (this.max ?? 0),
				),
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), null);
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), null);
	}

	protected override getFormElement() {
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	#removeItem(item: UmbDataTypeItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selection.length >= this.max) return nothing;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"></uui-button>
		`;
	}

	#renderItems() {
		if (!this._items) return nothing;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-ref-list>
		`;
	}

	#renderItem(item: UmbDataTypeItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node-data-type name=${item.name} id=${item.unique}>
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#removeItem(item)} label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node-data-type>
		`;
	}

	static override styles = [
		css`
			#btn-add {
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
