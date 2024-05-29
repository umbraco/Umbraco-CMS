import type { UmbDocumentTypeItemModel } from '../../repository/index.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_MODAL } from '../../workspace/document-type-workspace.modal-token.js';
import type { UmbDocumentTypeTreeItemModel } from '../../tree/types.js';
import { UmbDocumentTypePickerContext } from './input-document-type.context.js';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-document-type')
export class UmbInputDocumentTypeElement extends UUIFormControlMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputDocumentType',
		itemSelector: 'uui-ref-node-document-type',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * Limits to only select Element Types
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ attribute: false })
	elementTypesOnly: boolean = false;

	/**
	 * Limits to only select Document Types
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ attribute: false })
	documentTypesOnly: boolean = false;

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
	public set selection(uniques: Array<string>) {
		this.#pickerContext.setSelection(uniques);
		this.#sorter.setModel(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public set value(uniques: string) {
		this.selection = splitStringToArray(uniques);
	}
	public get value(): string {
		return this.selection.join(',');
	}
	@state()
	private _items?: Array<UmbDocumentTypeItemModel>;

	@state()
	private _editPath = '';

	#pickerContext = new UmbDocumentTypePickerContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_DOCUMENT_TYPE_WORKSPACE_MODAL)
			.addAdditionalPath('document-type')
			.onSetup(() => {
				return {};
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
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

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), '_observeSelection');
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observerItems');
	}

	protected getFormElement() {
		return undefined;
	}

	#getPickableFilter() {
		if (this.documentTypesOnly) {
			return (x: UmbDocumentTypeTreeItemModel) => x.isFolder === false && x.isElement === false;
		}
		if (this.elementTypesOnly) {
			return (x: UmbDocumentTypeTreeItemModel) => x.isElement;
		}
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: this.#getPickableFilter(),
		});
	}

	#removeItem(item: UmbDocumentTypeItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	render() {
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

	#renderItem(item: UmbDocumentTypeItemModel) {
		if (!item.unique) return;
		const href = `${this._editPath}edit/${item.unique}`;
		return html`
			<uui-ref-node-document-type name=${item.name} id=${item.unique}>
				${this.#renderIcon(item)}
				<uui-action-bar slot="actions">
					<uui-button href=${href} label=${this.localize.term('general_open')}></uui-button>
					<uui-button @click=${() => this.#removeItem(item)} label=${this.localize.term('general_remove')}></uui-button>
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
			#btn-add {
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
