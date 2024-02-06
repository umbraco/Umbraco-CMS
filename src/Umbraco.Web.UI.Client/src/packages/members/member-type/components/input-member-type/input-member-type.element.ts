import { UmbMemberTypePickerContext } from './input-member-type.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-member-type')
export class UmbInputMemberTypeElement extends FormControlMixin(UmbLitElement) {
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
		this.selectedIds = splitStringToArray(idsString);
	}

	@state()
	private _items?: Array<MemberTypeItemResponseModel>;

	#pickerContext = new UmbMemberTypePickerContext(this);

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

	#openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list
				>${repeat(
					this._items,
					(item) => item.id,
					(item) => this.#renderItem(item),
				)}</uui-ref-list
			>
		`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedIds.length >= this.max) return;
		return html`
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"
				>${this.localize.term('general_choose')}</uui-button
			>
		`;
	}

	#renderItem(item: MemberTypeItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node-document-type name=${ifDefined(item.name)}>
				${this.#renderIcon(item)}
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove Member Type ${item.name}"
						>${this.localize.term('general_remove')}</uui-button
					>
				</uui-action-bar>
			</uui-ref-node-document-type>
		`;
	}

	#renderIcon(item: MemberTypeItemResponseModel) {
		if (!item.icon) return;
		return html`<uui-icon slot="icon" name=${item.icon}></uui-icon>`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputMemberTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member-type': UmbInputMemberTypeElement;
	}
}
