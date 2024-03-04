import type { UmbMemberGroupItemModel } from '../../repository/index.js';
import { UmbMemberPickerContext } from './input-member-group.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { type UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

const SORTER_CONFIG: UmbSorterConfig<string> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('detail');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry;
	},
	identifier: 'Umb.SorterIdentifier.InputMemberGroup',
	itemSelector: 'uui-ref-node',
	containerSelector: 'uui-ref-list',
};

@customElement('umb-input-member-group')
export class UmbInputMemberGroupElement extends FormControlMixin(UmbLitElement) {
	#sorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this.selectedIds = model;
		},
	});

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
		this.#sorter.setModel(ids);
	}

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}

	@property({ type: Object, attribute: false })
	public filter: (memberGroup: UmbMemberGroupItemModel) => boolean = () => true;

	@state()
	private _editMemberGroupPath = '';

	@state()
	private _items?: Array<UmbMemberGroupItemModel>;

	#pickerContext = new UmbMemberPickerContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('member-group')
			.onSetup(() => {
				return { data: { entityType: 'member-group', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMemberGroupPath = routeBuilder({});
			});

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => {
			this._items = selectedItems;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

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
	}

	protected _openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	protected _requestRemoveItem(item: UmbMemberGroupItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique!);
	}

	protected getFormElement() {
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			filter: this.filter,
		});
	}

	#requestRemoveItem(item: MemberItemResponseModel) {
		this.#pickerContext.requestRemoveItem(item.id!);
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		return html`<uui-ref-list>
			${repeat(
				this._items,
				(item) => item.unique,
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

	#renderItem(item: UmbMemberGroupItemModel) {
		if (!item.unique) return;
		// TODO: get the correct variant name
		const name = item.name;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.unique)}>
				<uui-action-bar slot="actions">
					${this.#renderOpenButton(item)}
					<uui-button
						@click=${() => this._requestRemoveItem(item)}
						label="${this.localize.term('general_remove')} ${name}">
						${this.localize.term('general_remove')}
					</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderOpenButton(item: UmbMemberGroupItemModel) {
		if (!this.showOpenButton) return;
		// TODO: get the correct variant name
		const name = item.name;
		return html`
			<uui-button
				compact
				href="${this._editMemberGroupPath}edit/${item.unique}"
				label=${this.localize.term('general_edit') + ` ${name}`}>
				<uui-icon name="icon-edit"></uui-icon>
			</uui-button>
		`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}

			uui-ref-node[drag-placeholder] {
				opacity: 0.2;
			}
		`,
	];
}

export default UmbInputMemberGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member-group': UmbInputMemberGroupElement;
	}
}
