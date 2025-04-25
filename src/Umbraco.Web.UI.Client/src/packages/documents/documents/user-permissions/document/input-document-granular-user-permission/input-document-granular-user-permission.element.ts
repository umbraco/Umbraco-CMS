import type { UmbDocumentUserPermissionModel } from '../types.js';
import { UmbDocumentItemRepository } from '../../../item/index.js';
import type { UmbDocumentItemModel } from '../../../item/types.js';
import { UMB_DOCUMENT_PICKER_MODAL } from '../../../constants.js';
import { css, customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbChangeEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_ENTITY_USER_PERMISSION_MODAL,
	type ManifestEntityUserPermission,
} from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-input-document-granular-user-permission')
export class UmbInputDocumentGranularUserPermissionElement extends UUIFormControlMixin(UmbLitElement, '') {
	_permissions: Array<UmbDocumentUserPermissionModel> = [];
	public get permissions(): Array<UmbDocumentUserPermissionModel> {
		return this._permissions;
	}
	public set permissions(value: Array<UmbDocumentUserPermissionModel>) {
		this._permissions = value;
		const uniques = value.map((item) => item.document.id);
		this.#observePickedDocuments(uniques);
	}

	@property({ type: Array, attribute: false })
	fallbackPermissions: Array<string> = [];

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

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedDocuments(uniques: Array<string>) {
		const { asObservable } = await this.#documentItemRepository.requestItems(uniques);
		this.observe(asObservable?.(), (items) => (this._items = items), 'observeItems');
	}

	async #editGranularPermission(item: UmbDocumentItemModel) {
		const currentPermissionVerbs = this.#getPermissionForDocument(item.unique)?.verbs ?? [];
		const result = await this.#selectEntityUserPermissionsForDocument(item, currentPermissionVerbs);
		// don't do anything if the verbs have not been updated
		if (JSON.stringify(result) === JSON.stringify(currentPermissionVerbs)) return;

		// update permission with new verbs
		this.permissions = this._permissions.map((permission) => {
			if (permission.document.id === item.unique) {
				return {
					...permission,
					verbs: result,
				};
			}
			return permission;
		});

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #addGranularPermission() {
		this.#documentPickerModalContext = this.#modalManagerContext?.open(this, UMB_DOCUMENT_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				// prevent already selected items to be picked again
				// TODO: this type is wrong. It should be the tree item type
				pickableFilter: (treeItem: UmbDocumentItemModel) =>
					!this._items?.map((i) => i.unique).includes(treeItem.unique),
			},
		});

		this.#documentPickerModalContext?.addEventListener(UmbSelectedEvent.TYPE, async (event: UmbDeselectedEvent) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) return;

			const documentItem = await this.#requestDocumentItem(unique);

			this.#selectEntityUserPermissionsForDocument(documentItem).then(
				(result) => {
					this.#documentPickerModalContext?.reject();

					const permissionItem: UmbDocumentUserPermissionModel = {
						$type: 'DocumentPermissionPresentationModel',
						document: { id: unique },
						verbs: result,
					};

					this.permissions = [...this._permissions, permissionItem];
					this.dispatchEvent(new UmbChangeEvent());
				},
				() => {
					this.#documentPickerModalContext?.reject();
				},
			);
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
		const fallbackVerbs = this.#getFallbackPermissionVerbsForEntityType(item.entityType);
		const value = allowedVerbs.length > 0 ? { allowedVerbs } : undefined;
		this.#entityUserPermissionModalContext = this.#modalManagerContext?.open(this, UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				headline,
				preset: {
					allowedVerbs: fallbackVerbs,
				},
			},
			value,
		});

		try {
			// When the modal is submitted we return the new value from the modal
			const value = await this.#entityUserPermissionModalContext?.onSubmit();
			return value?.allowedVerbs;
		} catch {
			// When the modal is rejected we return the current value
			return allowedVerbs;
		}
	}

	#removeGranularPermission(item: UmbDocumentItemModel) {
		const permission = this.#getPermissionForDocument(item.unique);
		if (!permission) return;

		this.permissions = this._permissions.filter((v) => JSON.stringify(v) !== JSON.stringify(permission));
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderRef(item),
				)}
			</uui-ref-list>
		`;
	}

	#renderAddButton() {
		return html`<uui-button
			id="btn-add"
			look="placeholder"
			@click=${this.#addGranularPermission}
			label=${this.localize.term('general_add')}></uui-button>`;
	}

	#renderRef(item: UmbDocumentItemModel) {
		if (!item.unique) return;
		// TODO: get correct variant name
		const name = item.variants[0]?.name;
		const permissionNames = this.#getPermissionNamesForDocument(item.unique);

		return html`
			<uui-ref-node .name=${name} .detail=${permissionNames || ''}>
				${this.#renderIcon(item)} ${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					${this.#renderEditButton(item)} ${this.#renderRemoveButton(item)}
				</uui-action-bar>
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
		return html`
			<uui-button
				@click=${() => this.#editGranularPermission(item)}
				label=${this.localize.term('general_edit')}></uui-button>
		`;
	}

	#renderRemoveButton(item: UmbDocumentItemModel) {
		return html`<uui-button
			@click=${() => this.#removeGranularPermission(item)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	#getPermissionForDocument(unique: string) {
		return this._permissions?.find((permission) => permission.document.id === unique);
	}

	#getPermissionNamesForDocument(unique: string) {
		const permission = this.#getPermissionForDocument(unique);
		if (!permission) return;

		return umbExtensionsRegistry
			.getByTypeAndFilter('entityUserPermission', (manifest) =>
				manifest.meta.verbs.every((verb) => permission.verbs.includes(verb)),
			)
			.map((m) => {
				const manifest = m as ManifestEntityUserPermission;
				return manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
			})
			.join(', ');
	}

	#getFallbackPermissionVerbsForEntityType(entityType: string) {
		// get all permissions that are allowed for the entity type and have at least one of the fallback permissions
		// this is used to determine the default permissions for a document
		const verbs = umbExtensionsRegistry
			.getByTypeAndFilter(
				'entityUserPermission',
				(manifest) =>
					manifest.forEntityTypes.includes(entityType) &&
					this.fallbackPermissions.map((verb) => manifest.meta.verbs.includes(verb)).includes(true),
			)
			.flatMap((permission) => permission.meta.verbs);

		// ensure that the verbs are unique
		return [...new Set([...verbs])];
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentGranularUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-granular-user-permission': UmbInputDocumentGranularUserPermissionElement;
	}
}
