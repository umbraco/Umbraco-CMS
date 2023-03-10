import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from '../../../documents/documents/repository/document.tree.store';
import type { UmbDocumentTreeStore } from '../../../documents/documents/repository/document.tree.store';
import { UMB_CONFIRM_MODAL_TOKEN } from '../../modals/confirm';
import { UMB_DOCUMENT_PICKER_MODAL_TOKEN } from '../../../documents/documents/modals/document-picker';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DocumentTreeItemModel, FolderTreeItemModel } from '@umbraco-cms/backend-api';
import type { UmbObserverController } from '@umbraco-cms/observable-api';

@customElement('umb-input-document-picker')
export class UmbInputDocumentPickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
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

	@state()
	private _items?: Array<DocumentTreeItemModel>;

	private _modalContext?: UmbModalContext;
	private _documentStore?: UmbDocumentTreeStore;
	private _pickedItemsObserver?: UmbObserverController<FolderTreeItemModel>;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedKeys.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedKeys.length > this.max
		);

		this.consumeContext(UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this._documentStore = instance;
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

		if (!this._documentStore) return;

		// TODO: consider changing this to the list data endpoint when it is available
		this._pickedItemsObserver = this.observe(this._documentStore.items(this._selectedKeys), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		// We send a shallow copy(good enough as its just an array of keys) of our this._selectedKeys, as we don't want the modal to manipulate our data:
		const modalHandler = this._modalContext?.open(UMB_DOCUMENT_PICKER_MODAL_TOKEN, {
			multiple: this.max === 1 ? false : true,
			selection: [...this._selectedKeys],
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this._setSelection(selection);
		});
	}

	private async _removeItem(item: FolderTreeItemModel) {
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
			${this._items?.map((item) => this._renderItem(item))}
			<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>
		`;
	}

	private _renderItem(item: FolderTreeItemModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as FolderTreeItemModel & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.key)}>
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeItem(item)} label="Remove document ${item.name}">Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}
}

export default UmbInputDocumentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-picker': UmbInputDocumentPickerElement;
	}
}
