import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import { UmbDataTypePickerContext } from './data-type-input.context.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-data-type-input')
export class UmbDataTypeInputElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
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
	public set max(value: number) {
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Array })
	public set selection(ids: Array<string> | undefined) {
		this.#pickerContext.setSelection(ids ?? []);
	}
	public get selection(): Array<string> | undefined {
		return this.#pickerContext.getSelection();
	}

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selection = splitStringToArray(idsString);
	}
	public get value(): string {
		return this.selection?.join(',') ?? '';
	}

	@state()
	private _items?: Array<UmbDataTypeItemModel>;

	#pickerContext = new UmbDataTypePickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`
			<uui-ref-list>${this._items?.map((item) => this._renderItem(item))}</uui-ref-list>
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${() => this.#pickerContext.openPicker({ hideTreeRoot: true })}
				label=${this.localize.term('general_choose')}></uui-button>
		`;
	}

	private _renderItem(item: UmbDataTypeItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node-data-type name=${item.name}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node-data-type>
		`;
	}

	static styles = [
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
