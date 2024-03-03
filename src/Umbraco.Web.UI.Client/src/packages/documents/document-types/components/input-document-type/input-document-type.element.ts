import type { UmbDocumentTypeItemModel } from '../../repository/index.js';
import { UmbDocumentTypePickerContext } from './input-document-type.context.js';
import {
	css,
	html,
	customElement,
	property,
	state,
	ifDefined,
	repeat,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

@customElement('umb-input-document-type')
export class UmbInputDocumentTypeElement extends FormControlMixin(UmbLitElement) {
	/**
	 * Limits to only select Element Types
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean })
	elementTypesOnly: boolean = false;

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
	public set selectedIds(ids: Array<string> | undefined) {
		this.#pickerContext.setSelection(ids ?? []);
	}

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}
	public get value(): string {
		return this.selectedIds.join(',');
	}

	@state()
	private _items?: Array<UmbDocumentTypeItemModel>;

	@state()
	private _editDocumentTypePath = '';

	#pickerContext = new UmbDocumentTypePickerContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document-type')
			.onSetup(() => {
				return { data: { entityType: 'document-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentTypePath = routeBuilder({});
			});

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
		if (this.elementTypesOnly) {
			this.#pickerContext.openPicker({
				hideTreeRoot: true,
				pickableFilter: (x) => x.isElement,
			});
		} else {
			this.#pickerContext.openPicker({
				hideTreeRoot: true,
			});
		}
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return nothing;
		return html`
			<uui-ref-list
				>${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}</uui-ref-list
			>
		`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedIds.length >= this.max) return nothing;
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

	#renderItem(item: UmbDocumentTypeItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node-document-type name=${ifDefined(item.name)}>
				${this.#renderIcon(item)}
				<uui-action-bar slot="actions">
					<uui-button
						compact
						href=${this._editDocumentTypePath + 'edit/' + item.unique}
						label=${this.localize.term('general_edit') + ` ${item.name}`}>
						<uui-icon name="icon-edit"></uui-icon>
					</uui-button>
					<uui-button
						compact
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique)}
						label="Edit Document Type ${item.name}">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-ref-node-document-type>
		`;
	}

	#renderIcon(item: UmbDocumentTypeItemModel) {
		if (!item.icon) return;
		return html`<umb-icon slot="icon" name=${item.icon}></umb-icon>`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-type': UmbInputDocumentTypeElement;
	}
}
