import { UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from '../../repository/document.tree.store.js';
import type { UmbDocumentTreeStore } from '../../repository/document.tree.store.js';
import { css, html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles, FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
	UMB_DOCUMENT_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTreeItemResponseModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-document')
export class UmbInputDocumentElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

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
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	// TODO: do we need both selectedIds and value? If we just use value we follow the same pattern as native form controls.
	private _selectedIds: Array<string> = [];
	public get selectedIds(): Array<string> {
		return this._selectedIds;
	}
	public set selectedIds(ids: Array<string>) {
		this._selectedIds = ids;
		super.value = ids.join(',');
		this._observePickedDocuments();
	}

	@property()
	public set value(idsString: string) {
		if (idsString !== this._value) {
			this.selectedIds = idsString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<DocumentTreeItemResponseModel>;

	private _modalContext?: UmbModalManagerContext;
	private _documentStore?: UmbDocumentTreeStore;
	private _pickedItemsObserver?: UmbObserverController<EntityTreeItemResponseModel[]>;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedIds.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedIds.length > this.max
		);

		this.consumeContext(UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this._documentStore = instance;
			this._observePickedDocuments();
		});
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	protected getFormElement() {
		return undefined;
	}

	private _observePickedDocuments() {
		this._pickedItemsObserver?.destroy();

		if (!this._documentStore) return;

		// TODO: consider changing this to the list data endpoint when it is available
		this._pickedItemsObserver = this.observe(this._documentStore.items(this._selectedIds), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		// We send a shallow copy(good enough as its just an array of ids) of our this._selectedIds, as we don't want the modal to manipulate our data:
		const modalContext = this._modalContext?.open(UMB_DOCUMENT_PICKER_MODAL, {
			multiple: this.max === 1 ? false : true,
			selection: [...this._selectedIds],
		});

		modalContext?.onSubmit().then(({ selection }: any) => {
			this._setSelection(selection);
		});
	}

	private async _removeItem(item: EntityTreeItemResponseModel) {
		const modalContext = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalContext?.onSubmit();
		const newSelection = this._selectedIds.filter((value) => value !== item.id);
		this._setSelection(newSelection);
	}

	private _setSelection(newSelection: Array<string>) {
		this.selectedIds = newSelection;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html`
			${this._items?.map((item) => this._renderItem(item))}
			<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>
		`;
	}

	private _renderItem(item: EntityTreeItemResponseModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as EntityTreeItemResponseModel & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.id)}>
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeItem(item)} label="Remove document ${item.name}">Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document': UmbInputDocumentElement;
	}
}
