import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTypeResponseModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbDocumentTypeTreeStore,
	UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN,
} from '../../../documents/document-types/repository/document-type.tree.store';
import { UMB_CONFIRM_MODAL_TOKEN } from '../../modals/confirm';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL_TOKEN } from '../../../documents/documents/modals/document-type-picker';

@customElement('umb-input-document-type-picker')
export class UmbInputDocumentTypePickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}

			#current-node {
				background-color: var(--uui-color-surface-alt);
			}

			#wrapper-nodes {
				margin-left: var(--uui-size-space-6);
			}
		`,
	];

	// TODO: do we need both selectedKeys and value? If we just use value we follow the same pattern as native form controls.
	private _selectedKeys: Array<string> = [];
	public get selectedKeys(): Array<string> {
		return this._selectedKeys;
	}
	public set selectedKeys(keys: Array<string>) {
		this._selectedKeys = keys;
		super.value = keys.join(',');
		this._observePickedDocuments();
	}

	@property()
	public set value(keysString: string) {
		if (keysString !== this._value) {
			this.selectedKeys = keysString.split(/[ ,]+/);
		}
	}

	@property()
	currentDocumentType?: DocumentTypeResponseModel;

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
		this._pickedItemsObserver = this.observe(this._documentTypeStore.items(this._selectedKeys), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		// We send a shallow copy(good enough as its just an array of keys) of our this._selectedKeys, as we don't want the modal to manipulate our data:
		const modalHandler = this._modalContext?.open(UMB_DOCUMENT_TYPE_PICKER_MODAL_TOKEN, {
			multiple: true,
			selection: [...this._selectedKeys],
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this._setSelection(selection);
		});
	}

	private async _removeItem(item: EntityTreeItemResponseModel) {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL_TOKEN, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalHandler?.onSubmit();
		const newSelection = this._selectedKeys.filter((value) => value !== item.key);
		this._setSelection(newSelection);
	}

	private _setSelection(newSelection: Array<string>) {
		this.selectedKeys = newSelection;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html`
			<uui-ref-node id="current-node" .name="${this.currentDocumentType?.name ?? ''} (current)">
				<uui-icon slot="icon" .name="${this.currentDocumentType?.icon ?? 'umb:document'}"></uui-icon>
			</uui-ref-node>
			<div id="wrapper-nodes">
				<uui-ref-list> ${this._items?.map((item) => this._renderItem(item))} </uui-ref-list>
				<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>
			</div>
		`;
	}

	private _renderItem(item: EntityTreeItemResponseModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as EntityTreeItemResponseModel & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.key)}>
				<uui-icon slot="icon" name="${ifDefined(item.icon)}"></uui-icon>
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeItem(item)} label="Remove document ${item.name}">Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}
}

export default UmbInputDocumentTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-type-picker': UmbInputDocumentTypePickerElement;
	}
}
