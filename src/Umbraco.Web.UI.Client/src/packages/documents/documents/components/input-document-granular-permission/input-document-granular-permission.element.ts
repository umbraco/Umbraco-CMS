import { css, html, customElement, property, state, PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
	UMB_DOCUMENT_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-document-granular-permission')
export class UmbInputDocumentGranularPermissionElement extends FormControlMixin(UmbLitElement) {
	private _selectedIds: Array<string> = [];
	public get selectedIds(): Array<string> {
		return this._selectedIds;
	}
	public set selectedIds(ids: Array<string>) {
		this._selectedIds = ids;
		super.value = ids.join(',');
		this.#observePickedDocuments();
	}

	@property()
	public set value(idsString: string) {
		if (idsString !== this._value) {
			this.selectedIds = idsString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<DocumentItemResponseModel>;

	#documentRepository = new UmbDocumentRepository(this);
	#modalContext?: UmbModalManagerContext;
	#pickedItemsObserver?: UmbObserverController<Array<DocumentItemResponseModel>>;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => (this.#modalContext = instance));
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.#observePickedDocuments();
	}

	protected getFormElement() {
		return undefined;
	}

	async #observePickedDocuments() {
		this.#pickedItemsObserver?.destroy();

		const { asObservable } = await this.#documentRepository.requestItems(this._selectedIds);
		this.#pickedItemsObserver = this.observe(asObservable(), (items) => (this._items = items));
	}

	#openDocumentPicker() {
		// We send a shallow copy(good enough as its just an array of ids) of our this._selectedIds, as we don't want the modal to manipulate our data:
		const modalContext = this.#modalContext?.open(UMB_DOCUMENT_PICKER_MODAL, {
			selection: [...this._selectedIds],
		});

		modalContext?.onSubmit().then(({ selection }: any) => {
			//this.#setSelection(selection);
		});
	}

	async #removeItem(item: DocumentItemResponseModel) {
		const modalContext = this.#modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalContext?.onSubmit();
		const newSelection = this._selectedIds.filter((value) => value !== item.id);
		this.#setSelection(newSelection);
	}

	#setSelection(newSelection: Array<string>) {
		this.selectedIds = newSelection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#pickedItemsObserver?.destroy();
	}

	render() {
		return html`
			${this._items?.map((item) => this.#renderItem(item))}
			<uui-button id="add-button" look="placeholder" @click=${this.#openDocumentPicker} label="open">Add</uui-button>
		`;
	}

	#renderItem(item: DocumentItemResponseModel) {
		return html` <div>Render something here</div> `;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentGranularPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-granular-permission': UmbInputDocumentGranularPermissionElement;
	}
}
