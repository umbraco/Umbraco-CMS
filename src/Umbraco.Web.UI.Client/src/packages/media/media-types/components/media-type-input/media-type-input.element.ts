import { UmbMediaTypePickerContext } from './media-type-input.context.js';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-media-type-input')
export class UmbMediaTypeInputElement extends FormControlMixin(UmbLitElement) {
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
	private _items?: Array<MediaTypeItemResponseModel>;

	#pickerContext = new UmbMediaTypePickerContext(this);

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

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		console.log('ITEMS', this._items);
		return html`
			<uui-ref-list>${this._items?.map((item) => this._renderItem(item))}</uui-ref-list>
			<uui-button id="add-button" look="placeholder" @click=${() => this.#pickerContext.openPicker()} label="open"
				>Add</uui-button
			>
		`;
	}

	private _renderItem(item: MediaTypeItemResponseModel) {
		if (!item.id) return;

		//TODO: Using uui-ref-node as we don't have a uui-ref-media-type yet.
		return html`
			<uui-ref-node name=${ifDefined(item.name)}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove Media Type ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
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

export default UmbMediaTypeInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-input': UmbMediaTypeInputElement;
	}
}
