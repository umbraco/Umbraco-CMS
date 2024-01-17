import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

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

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}

	@state()
	private _editMemberPath = '';

	@state()
	private _items?: Array<MemberItemResponseModel>;

	// TODO: Create the `UmbMemberPickerContext` [LK]
	//#pickerContext = new UmbMemberPickerContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('member')
			.onSetup(() => {
				return { data: { entityType: 'member', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMemberPath = routeBuilder({});
			});
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

	protected _openPicker() {
		console.log("member.openPicker");
		// this.#pickerContext.openPicker({
		// 	hideTreeRoot: true,
		// });
	}

	protected _requestRemoveItem(item: MemberItemResponseModel) {
		console.log("member.requestRemoveItem", item);
		//this.#pickerContext.requestRemoveItem(item.id!);
	}

	protected getFormElement() {
		return undefined;
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
				(item) => this._renderItem(item),
			)}
		</uui-ref-list>`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedIds.length >= this.max) return;
		return html`<uui-button
			id="add-button"
			look="placeholder"
			@click=${this._openPicker}
			label=${this.localize.term('general_add')}></uui-button>`;
	}

	private _renderItem(item: MemberItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				<!-- TODO: implement is deleted <uui-tag size="s" slot="tag" color="danger">Deleted</uui-tag> -->
				<uui-action-bar slot="actions">
					${this.#renderOpenButton(item)}
					<uui-button
						@click=${() => this._requestRemoveItem(item)}
						label="${this.localize.term('general_remove')} ${item.name}">
						${this.localize.term('general_remove')}
					</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderOpenButton(item: MemberItemResponseModel) {
		if (!this.showOpenButton) return;
		return html`
			<uui-button
				compact
				href="${this._editMemberPath}edit/${item.id}"
				label=${this.localize.term('general_edit') + ` ${item.name}`}>
				<uui-icon name="icon-edit"></uui-icon>
			</uui-button>
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

export default UmbInputMemberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member': UmbInputMemberElement;
	}
}
