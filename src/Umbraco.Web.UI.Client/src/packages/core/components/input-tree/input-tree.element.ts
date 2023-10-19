import { NodeType, UmbNodePickerContext } from './input-tree.context.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-tree')
export class UmbInputTreeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

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
	public set type(value: NodeType) {
		this.#pickerContext.setNodeType(value);
	}
	public get type(): NodeType {
		return this.#pickerContext.nodeType;
	}

	@state()
	private _items?: Array<ItemResponseModelBaseModel>;

	#pickerContext: UmbNodePickerContext;

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

		this.#pickerContext = new UmbNodePickerContext(this);

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	render() {
		return html` ${this._items?.map((item) => this.#renderItem(item))} ${this.#renderButton()} `;
	}

	#renderButton() {
		if (this._items && this.max && this._items.length >= this.max) return;
		return html`
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${() => this.#pickerContext.openPicker()}
				label=${this.localize.term('general_add')}>
				${this.localize.term('general_add')}
			</uui-button>
		`;
	}

	#renderItem(item: ItemResponseModelBaseModel) {
		const icon = this.type === 'media' ? 'umb:picture' : this.type === 'member' ? 'umb:user' : 'umb:document';
		return html`<uui-ref-node .name=${item.name ?? ''} .detail=${item.id ?? ''}>
			<uui-icon slot="icon" name=${icon}></uui-icon>
		</uui-ref-node>`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}
		`,
	];
}

export default UmbInputTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tree': UmbInputTreeElement;
	}
}
