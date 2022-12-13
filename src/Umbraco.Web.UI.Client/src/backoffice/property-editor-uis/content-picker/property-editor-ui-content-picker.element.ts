import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import type { UmbModalService } from '../../../core/services/modal';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbDocumentStore } from 'src/core/stores/document/document.store';
import { FolderTreeItem } from '@umbraco-cms/backend-api';

@customElement('umb-property-editor-ui-content-picker')
export class UmbPropertyEditorUIContentPickerElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}

			#add-button {
				width: 100%;
			}
		`,
	];

	@property({ type: Array })
	public value: Array<string> = [];

	@property({ type: Array, attribute: false })
	public config = [];

	@state()
	private _items: Array<FolderTreeItem> = [];

	private _modalService?: UmbModalService;
	private _documentStore?: UmbDocumentStore;

	constructor() {
		super();

		this.consumeAllContexts(['umbDocumentStore', 'umbModalService'], (instances) => {
			this._documentStore = instances['umbDocumentStore'];
			this._modalService = instances['umbModalService'];
			this._observePickedDocuments();
		});
	}

	private _observePickedDocuments() {
		if (!this._documentStore) return;
		// TODO: consider changing this to the list data endpoint when it is available
		this.observe<FolderTreeItem[]>(this._documentStore.getTreeItems(this.value), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		const modalHandler = this._modalService?.contentPicker({ multiple: true, selection: this.value });
		modalHandler?.onClose().then(({ selection }: any) => {
			this._setValue([...selection]);
		});
	}

	private _removeItem(item: FolderTreeItem) {
		const modalHandler = this._modalService?.confirm({
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		modalHandler?.onClose().then(({ confirmed }) => {
			if (confirmed) {
				const newValue = this.value.filter((value) => value !== item.key);
				this._setValue(newValue);
			}
		});
	}

	private _setValue(newValue: Array<string>) {
		this.value = newValue;
		this._observePickedDocuments();
		this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
	}

	private _renderItem(item: FolderTreeItem) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as FolderTreeItem & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.key)}>
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeItem(item)}>Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	render() {
		return html`${this._items.map((item) => this._renderItem(item))}
			<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>`;
	}
}

export default UmbPropertyEditorUIContentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-content-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
