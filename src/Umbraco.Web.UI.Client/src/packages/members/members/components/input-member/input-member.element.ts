import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-member')
export class UmbInputMemberElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public get min(): number {
		//return this.#pickerContext.min;
		return 0;
	}
	public set min(value: number) {
		//this.#pickerContext.min = value;
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
		//return this.#pickerContext.max;
		return Infinity;
	}
	public set max(value: number) {
		//this.#pickerContext.max = value;
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
		//return this.#pickerContext.getSelection();
		return [];
	}
	public set selectedIds(ids: Array<string>) {
		//this.#pickerContext.setSelection(ids);
	}

	@property({ type: Array })
	filter?: string[] | undefined;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}

	@state()
	private _items?: Array<MemberItemResponseModel>;

	// TODO: Create the `UmbMemberPickerContext` [LK]
	//#pickerContext = new UmbMemberPickerContext(this);

	constructor() {
		super();

		// this.addValidator(
		// 	'rangeUnderflow',
		// 	() => this.minMessage,
		// 	() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		// );

		// this.addValidator(
		// 	'rangeOverflow',
		// 	() => this.maxMessage,
		// 	() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		// );

		// this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		// this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}
	protected getFormElement() {
		return undefined;
	}

	#pickableFilter: (item: MemberItemResponseModel) => boolean = (item) => {
		// TODO: Uncomment, once `UmbMemberPickerContext` has been implemented. [LK]
		console.log('member.pickableFilter', item);
		// 	if (this.filter && this.filter.length > 0) {
		// 		return this.filter.includes(item.contentTypeId);
		// 	}
		return true;
	};

	#openPicker() {
		console.log('member.openPicker');
		// this.#pickerContext.openPicker({
		// 	hideTreeRoot: true,
		//	pickableFilter: this.#pickableFilter,
		// });
	}

	#requestRemoveItem(item: MemberItemResponseModel) {
		console.log('member.requestRemoveItem', item);
		//this.#pickerContext.requestRemoveItem(item.id!);
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		// TODO: Add sorting. [LK]
		return html`<uui-ref-list
			>${repeat(
				this._items,
				(item) => item.id,
				(item) => this.#renderItem(item),
			)}
		</uui-ref-list>`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedIds.length >= this.max) return;
		return html`<uui-button
			id="add-button"
			look="placeholder"
			@click=${this.#openPicker}
			label=${this.localize.term('general_choose')}></uui-button>`;
	}

	#renderItem(item: MemberItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#requestRemoveItem(item)} label="Remove member ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIsTrashed(item: MemberItemResponseModel) {
		// TODO: Uncomment, once the Management API model support deleted members. [LK]
		//if (!item.isTrashed) return;
		//return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputMemberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member': UmbInputMemberElement;
	}
}
