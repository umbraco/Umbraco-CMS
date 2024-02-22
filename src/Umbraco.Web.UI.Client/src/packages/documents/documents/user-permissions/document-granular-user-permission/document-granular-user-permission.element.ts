import type { UmbDocumentUserPermissionModel } from '../types.js';
import { UmbDocumentItemRepository, type UmbDocumentItemModel } from '../../repository/index.js';
import { css, customElement, html, ifDefined, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import {
	UMB_DOCUMENT_PICKER_MODAL,
	UMB_ENTITY_USER_PERMISSION_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
} from '@umbraco-cms/backoffice/modal';
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
	#documentPickerModalContext?: any;
	#entityUserPermissionModalContext?: any;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => (this.#modalManagerContext = instance));
	}

	async #observePickedDocuments(uniques: Array<string>) {
		const { asObservable } = await this.#documentItemRepository.requestItems(uniques);
		this.observe(asObservable(), (items) => (this._items = items));
	}

	async #editGranularPermission(item: UmbDocumentItemModel) {
		const currentPermissionVerbs = this.#getPermissionForDocument(item.unique)?.verbs ?? [];
		const result = await this.#selectEntityUserPermissionsForDocument(item, currentPermissionVerbs);

		// update permission with new verbs
		this.value = this._value.map((permission) => {
			if (permission.document.id === item.unique) {
				return {
					...permission,
					verbs: result,
				};
			}
			return permission;
		});
	}

	#addGranularPermission() {
		this.#documentPickerModalContext = this.#modalManagerContext?.open(UMB_DOCUMENT_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				// prevent already selected items to be picked again
				// TODO: this type is wrong. It should be the tree item type
				pickableFilter: (treeItem: UmbDocumentItemModel) =>
					!this._items?.map((i) => i.unique).includes(treeItem.unique),
			},
		});

		this.#documentPickerModalContext?.addEventListener(UmbSelectedEvent.TYPE, async (event) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) return;

			const documentItem = await this.#requestDocumentItem(unique);
			const result = await this.#selectEntityUserPermissionsForDocument(documentItem);
			this.#documentPickerModalContext?.reject();

			const permissionItem: UmbDocumentUserPermissionModel = {
				$type: 'DocumentPermissionModel',
				document: { id: unique },
				verbs: [],
			};

			this._value = [...this._value, permissionItem];
			this.requestUpdate();
		});
	}

	async #requestDocumentItem(unique: string) {
		if (!unique) throw new Error('Could not open permissions modal, no unique was provided');

		const { data } = await this.#documentItemRepository.requestItems([unique]);

		const documentItem = data?.[0];
		if (!documentItem) throw new Error('No document item found');
		return documentItem;
	}

	async #selectEntityUserPermissionsForDocument(item: UmbDocumentItemModel, allowedVerbs: Array<string> = []) {
		// TODO: get correct variant name
		const name = item.variants[0]?.name;
		const headline = name ? `Permissions for ${name}` : 'Permissions';

		this.#entityUserPermissionModalContext = this.#modalManagerContext?.open(UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				headline,
			},
			value: {
				allowedVerbs,
			},
		});

		try {
			const value = await this.#entityUserPermissionModalContext?.onSubmit();
			return value?.allowedVerbs;
		} catch (error) {
			return [];
		}
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
			@click=${this.#addGranularPermission}
			label=${this.localize.term('general_add')}></uui-button>`;
	}

	// TODO: make umb-document-ref element
	#renderRef(item: UmbDocumentItemModel) {
		if (!item.unique) return;
		// TODO: get correct variant name
		const name = item.variants[0]?.name;

		return html`
			<uui-ref-node name=${name} detail=${ifDefined(this.#getPermissionForDocument(item.unique)?.verbs)}>
				${this.#renderIcon(item)} ${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions"> ${this.#renderEditButton(item)} </uui-action-bar>
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

	#renderEditButton(item: UmbDocumentItemModel) {
		// TODO: get correct variant name
		const name = item.variants[0]?.name;

		return html`
			<uui-button
				@click=${() => this.#editGranularPermission(item)}
				label="${this.localize.term('general_edit')} ${name}"
				>${this.localize.term('general_edit')}</uui-button
			>
		`;
	}

	#getPermissionForDocument(unique: string) {
		return this._value?.find((permission) => permission.document.id === unique);
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
