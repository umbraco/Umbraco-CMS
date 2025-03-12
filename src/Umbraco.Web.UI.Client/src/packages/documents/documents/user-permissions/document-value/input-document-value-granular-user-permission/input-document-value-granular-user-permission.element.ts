import type { UmbDocumentValueUserPermissionModel } from '../types.js';
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
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import {
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	UMB_DOCUMENT_TYPE_PROPERTY_PICKER_MODAL,
	UmbDocumentTypeDetailRepository,
	UmbDocumentTypeItemRepository,
	type UmbDocumentTypeItemModel,
	type UmbDocumentTypeTreeItemModel,
} from '@umbraco-cms/backoffice/document-type';

@customElement('umb-document-value-granular-user-permission')
export class UmbInputDocumentValueGranularUserPermissionElement extends UUIFormControlMixin(UmbLitElement, '') {
	_permissions: Array<UmbDocumentValueUserPermissionModel> = [];
	public get permissions(): Array<UmbDocumentValueUserPermissionModel> {
		return this._permissions;
	}
	public set permissions(value: Array<UmbDocumentValueUserPermissionModel>) {
		this._permissions = value;
		const uniques = value.map((item) => item.documentType.unique);
		this.#observePickedDocumentTypes(uniques);
	}

	@property({ type: Array, attribute: false })
	fallbackPermissions: Array<string> = [];

	@state()
	private _items?: Array<UmbDocumentTypeItemModel>;

	#documentTypeItemRepository = new UmbDocumentTypeItemRepository(this);
	#documentTypeDetailRepository = new UmbDocumentTypeDetailRepository(this);
	#modalManagerContext?: UmbModalManagerContext;
	#documentTypePickerModalContext?: any;
	#documentTypePropertyPickerModalContext?: any;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => (this.#modalManagerContext = instance));
	}

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedDocumentTypes(uniques: Array<string>) {
		const { asObservable } = await this.#documentTypeItemRepository.requestItems(uniques);
		this.observe(asObservable(), (items) => (this._items = items));
	}

	/*
	async #editGranularPermission(item: UmbDocumentTypeItemModel) {
		const currentPermissionVerbs = this.#getPermissionForDocumentType(item.unique)?.verbs ?? [];
		const result = await this.#selectEntityUserPermissionsForDocumentType(item, currentPermissionVerbs);
		// don't do anything if the verbs have not been updated
		if (JSON.stringify(result) === JSON.stringify(currentPermissionVerbs)) return;

		// update permission with new verbs
		this.permissions = this._permissions.map((permission) => {
			if (permission.documentType.unique === item.unique) {
				return {
					...permission,
					verbs: result,
				};
			}
			return permission;
		});

		this.dispatchEvent(new UmbChangeEvent());
	}
		*/

	async #addGranularPermission() {
		this.#documentTypePickerModalContext = this.#modalManagerContext?.open(this, UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				// prevent already selected items to be picked again
				pickableFilter: (treeItem: UmbDocumentTypeTreeItemModel) =>
					!this._items?.map((i) => i.unique).includes(treeItem.unique),
			},
		});

		this.#documentTypePickerModalContext?.addEventListener(UmbSelectedEvent.TYPE, async (event: UmbDeselectedEvent) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) {
				throw new Error('Could not open Document type property modal, no unique was provided');
			}

			const documentTypeItem = await this.#requestDocumentTypeItem(unique);

			this.#documentTypePropertyPickerModalContext = this.#modalManagerContext?.open(
				this,
				UMB_DOCUMENT_TYPE_PROPERTY_PICKER_MODAL,
				{
					data: {
						documentType: {
							unique: documentTypeItem.unique,
						},
					},
				},
			);

			this.#documentTypePropertyPickerModalContext?.addEventListener(
				UmbSelectedEvent.TYPE,
				async (event: UmbSelectedEvent) => {
					const selectedEvent = event as UmbSelectedEvent;
					const propertyAlias = selectedEvent.unique;

					if (!propertyAlias) {
						throw new Error('Could not open permissions modal, no property alias was provided');
					}

					const { data: documentTypeDetails } = await this.#documentTypeDetailRepository.requestByUnique(unique);
					const property = documentTypeDetails?.properties.find((p) => p.alias === propertyAlias);

					if (!property) {
						throw new Error('Could not open permissions modal, no property was found');
					}

					this.#selectEntityUserPermissionsForProperty(property).then(
						(result) => {
							this.#documentTypePickerModalContext?.reject();
							this.#documentTypePropertyPickerModalContext?.reject();

							const permissionItem: UmbDocumentTypeStructureUserPermissionModel = {
								$type: 'DocumentTypeStructurePermissionPresentationModel',
								documentType: { unique },
								property: { alias: propertyAlias },
								verbs: result,
							};

							this.permissions = [...this._permissions, permissionItem];
							this.dispatchEvent(new UmbChangeEvent());
						},
						() => {
							this.#documentTypePickerModalContext?.reject();
						},
					);
				},
			);
		});
	}

	async #requestDocumentTypeItem(unique: string) {
		if (!unique) throw new Error('Could not open permissions modal, no unique was provided');

		const { data } = await this.#documentTypeItemRepository.requestItems([unique]);

		const documentItem = data?.[0];
		if (!documentItem) throw new Error('No document item found');
		return documentItem;
	}

	async #selectEntityUserPermissionsForProperty(property: UmbPropertyTypeModel, allowedVerbs: Array<string> = []) {
		// TODO: get correct variant name
		const name = property.name;
		const headline = name ? `Permissions for ${name}` : 'Permissions';
		const fallbackVerbs = this.#getFallbackPermissionVerbsForEntityType('document-type-property');

		this.#documentTypePropertyPickerModalContext = this.#modalManagerContext?.open(
			this,
			UMB_ENTITY_USER_PERMISSION_MODAL,
			{
				data: {
					unique: property.alias,
					entityType: 'document-type-property',
					headline,
					preset: {
						allowedVerbs: fallbackVerbs,
					},
				},
				value: {
					allowedVerbs,
				},
			},
		);

		try {
			const value = await this.#documentTypePropertyPickerModalContext?.onSubmit();
			return value?.allowedVerbs;
		} catch {
			throw new Error();
		}
	}

	#removeGranularPermission(item: UmbDocumentTypeItemModel) {
		const permission = this.#getPermissionForDocumentType(item.unique);
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

	#renderRef(item: UmbDocumentTypeItemModel) {
		if (!item.unique) return;
		const name = item.name;
		const permissionNames = this.#getPermissionNamesForDocumentType(item.unique);

		return html`
			<uui-ref-node .name=${name} .detail=${permissionNames || ''}>
				${this.#renderIcon(item)}
				<uui-action-bar slot="actions">${this.#renderRemoveButton(item)}</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDocumentTypeItemModel) {
		if (!item.icon) return;
		return html`<uui-icon slot="icon" name=${item.icon}></uui-icon>`;
	}

	#renderRemoveButton(item: UmbDocumentTypeItemModel) {
		return html`<uui-button
			@click=${() => this.#removeGranularPermission(item)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	#getPermissionForDocumentType(unique: string) {
		return this._permissions?.find((permission) => permission.documentType.unique === unique);
	}

	#getPermissionNamesForDocumentType(unique: string) {
		const permission = this.#getPermissionForDocumentType(unique);
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

export { UmbInputDocumentValueGranularUserPermissionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-value-granular-user-permission': UmbInputDocumentValueGranularUserPermissionElement;
	}
}
