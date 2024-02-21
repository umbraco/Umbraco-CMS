import type { UmbDocumentUserPermissionModel } from '../types.js';
import { UmbDocumentItemRepository, type UmbDocumentItemModel } from '../../repository/index.js';
import { css, customElement, html, ifDefined, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { ManifestGranularUserPermission } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-granular-user-permission')
export class UmbDocumentGranularUserPermissionElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestGranularUserPermission;

	_value: Array<UmbDocumentUserPermissionModel> = [];
	public get value(): Array<UmbDocumentUserPermissionModel> {
		return this._value;
	}
	public set value(value: Array<UmbDocumentUserPermissionModel>) {
		this._value = value;
		const uniques = value.map((item) => item.document.id);
		this.#observePickedDocuments(uniques);
	}

	@state()
	private _items?: Array<UmbDocumentItemModel>;

	#documentItemRepository = new UmbDocumentItemRepository(this);
	#modalManagerContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => (this.#modalManagerContext = instance));
	}

	async #observePickedDocuments(uniques: Array<string>) {
		const { asObservable } = await this.#documentItemRepository.requestItems(uniques);
		this.observe(asObservable(), (items) => (this._items = items));
	}

	#openPicker() {
		const modalContext = this.#modalManagerContext?.open(UMB_DOCUMENT_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
			},
		});

		modalContext?.addEventListener(UmbSelectedEvent.TYPE, (event) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) return;
			console.log(unique);
		});

		modalContext?.onSubmit().then((value) => {
			//this.#setSelection(selection);
		});
	}

	render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`<uui-ref-list>
			${repeat(
				this._items,
				(item) => item.unique,
				(item) => this.#renderRef(item),
			)}
		</uui-ref-list>`;
	}

	#renderAddButton() {
		return html`<uui-button
			id="add-button"
			look="placeholder"
			@click=${this.#openPicker}
			label=${this.localize.term('general_choose')}></uui-button>`;
	}

	// TODO: make umb-document-ref element
	#renderRef(item: UmbDocumentItemModel) {
		if (!item.unique) return;
		// TODO: get correct variant name
		const name = item.variants[0]?.name;

		return html`
			<uui-ref-node name=${name} detail=${ifDefined(this.#getPermissionVerbsForItem(item))}>
				${this.#renderIcon(item)} ${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions"> </uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDocumentItemModel) {
		if (!item.documentType.icon) return;
		return html`<uui-icon slot="icon" name=${item.documentType.icon}></uui-icon>`;
	}

	#renderIsTrashed(item: UmbDocumentItemModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	#getPermissionVerbsForItem(item: UmbDocumentItemModel) {
		const permission = this._value?.find((permission) => permission.document.id === item.unique);
		return permission?.verbs;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbDocumentGranularUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-granular-user-permission': UmbDocumentGranularUserPermissionElement;
	}
}
