import {
	UmbDocumentTypeTreeStore,
	UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN,
} from '../../repository/document-type.tree.store.js';
import { css, html, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles, FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { DocumentTypeResponseModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-document-type-picker')
export class UmbInputDocumentTypePickerElement extends FormControlMixin(UmbLitElement) {
	// TODO: do we need both selectedIds and value? If we just use value we follow the same pattern as native form controls.
	private _selectedIds: Array<string> = [];
	@property({ type: Array })
	public get selectedIds(): Array<string> {
		return this._selectedIds;
	}
	public set selectedIds(ids: Array<string>) {
		this._selectedIds = ids ?? [];
		super.value = this._selectedIds.join(',');
		this._observePickedDocuments();
	}

	@property()
	public set value(idsString: string) {
		if (idsString !== this._value) {
			this.selectedIds = idsString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<DocumentTypeResponseModel>;

	private _modalContext?: UmbModalContext;
	private _documentTypeStore?: UmbDocumentTypeTreeStore;
	private _pickedItemsObserver?: UmbObserverController<EntityTreeItemResponseModel[]>;

	constructor() {
		super();
		this.consumeContext(UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this._documentTypeStore = instance;
			this._observePickedDocuments();
		});
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	protected getFormElement() {
		return undefined;
	}

	private _observePickedDocuments() {
		this._pickedItemsObserver?.destroy();

		if (!this._documentTypeStore) return;

		// TODO: consider changing this to the list data endpoint when it is available
		this._pickedItemsObserver = this.observe(this._documentTypeStore.items(this._selectedIds), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		// We send a shallow copy(good enough as its just an array of ids) of our this._selectedIds, as we don't want the modal to manipulate our data:
		const modalHandler = this._modalContext?.open(UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			multiple: true,
			selection: [...this._selectedIds],
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this._setSelection(selection);
		});
	}

	private async _removeItem(item: DocumentTypeResponseModel) {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalHandler?.onSubmit();
		const newSelection = this._selectedIds.filter((value) => value !== item.id);
		this._setSelection(newSelection);
	}

	private _setSelection(newSelection: Array<string>) {
		this.selectedIds = newSelection;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html`
			<uui-ref-list> ${this._items?.map((item) => this._renderItem(item))} </uui-ref-list>
			<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>
		`;
	}

	private _renderItem(item: DocumentTypeResponseModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as DocumentTypeResponseModel & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.id)}>
				<uui-icon slot="icon" name="${ifDefined(item.icon)}"></uui-icon>
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

export default UmbInputDocumentTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-type-picker': UmbInputDocumentTypePickerElement;
	}
}
