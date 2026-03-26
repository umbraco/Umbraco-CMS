import { UmbElementFolderItemRepository } from '../folder/repository/item/element-folder-item.repository.js';
import { UMB_ELEMENT_PICKER_MODAL } from '../modals/element-picker-modal.token.js';
import type { UmbElementFolderItemModel } from '../folder/repository/item/types.js';
import type { UmbElementTreeItemModel } from '../tree/types.js';
import type { UmbElementUserPermissionModel } from './types.js';
import { css, customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UMB_ENTITY_USER_PERMISSION_MODAL } from '@umbraco-cms/backoffice/user-permission';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestEntityUserPermission } from '@umbraco-cms/backoffice/user-permission';
import type { UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-input-element-folder-granular-user-permission')
export class UmbInputElementFolderGranularUserPermissionElement extends UUIFormControlMixin(UmbLitElement, '') {
	#permissions: Array<UmbElementUserPermissionModel> = [];
	public get permissions(): Array<UmbElementUserPermissionModel> {
		return this.#permissions;
	}
	public set permissions(value: Array<UmbElementUserPermissionModel>) {
		this.#permissions = value;
		const uniques = value.map((item) => item.element.id);
		this.#observePickedFolders(uniques);
	}

	@property({ type: Array, attribute: false })
	public fallbackPermissions: Array<string> = [];

	@state()
	private _items?: Array<UmbElementFolderItemModel>;

	#folderItemRepository = new UmbElementFolderItemRepository(this);
	#modalManagerContext?: UmbModalManagerContext;
	#folderPickerModalContext?: any;
	#entityUserPermissionModalContext?: any;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => (this.#modalManagerContext = instance));
	}

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedFolders(uniques: Array<string>) {
		const { asObservable } = await this.#folderItemRepository.requestItems(uniques);
		this.observe(
			asObservable?.(),
			(items) => {
				this._items = items;
			},
			'observeItems',
		);
	}

	async #editGranularPermission(item: UmbElementFolderItemModel) {
		const currentPermissionVerbs = this.#getPermissionForFolder(item.unique)?.verbs ?? [];
		const result = await this.#selectEntityUserPermissionsForFolder(item, currentPermissionVerbs);
		// don't do anything if the verbs have not been updated
		if (JSON.stringify(result) === JSON.stringify(currentPermissionVerbs)) return;

		// update permission with new verbs
		this.permissions = this.#permissions.map((permission) => {
			if (permission.element.id === item.unique) {
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
		this.#folderPickerModalContext = this.#modalManagerContext?.open(this, UMB_ELEMENT_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				foldersOnly: true,
				// prevent already selected items to be picked again
				pickableFilter: (treeItem: UmbElementTreeItemModel) =>
					!this._items?.map((i) => i.unique).includes(treeItem.unique),
			},
		});

		this.#folderPickerModalContext?.addEventListener(UmbSelectedEvent.TYPE, async (event: UmbDeselectedEvent) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) return;

			const folderItem = await this.#requestFolderItem(unique);

			this.#selectEntityUserPermissionsForFolder(folderItem).then(
				(result) => {
					this.#folderPickerModalContext?.reject();

					const permissionItem: UmbElementUserPermissionModel = {
						$type: 'ElementPermissionPresentationModel',
						element: { id: unique },
						verbs: result,
					};

					this.permissions = [...this.#permissions, permissionItem];
					this.dispatchEvent(new UmbChangeEvent());
				},
				() => {
					this.#folderPickerModalContext?.reject();
				},
			);
		});
	}

	async #requestFolderItem(unique: string) {
		if (!unique) throw new Error('Could not open permissions modal, no unique was provided');

		const { data } = await this.#folderItemRepository.requestItems([unique]);

		const folderItem = data?.[0];
		if (!folderItem) throw new Error('No element folder item found');
		return folderItem;
	}

	async #selectEntityUserPermissionsForFolder(item: UmbElementFolderItemModel, allowedVerbs: Array<string> = []) {
		const headline = item.name ? `Permissions for ${item.name}` : 'Permissions';
		const fallbackVerbs = this.#getFallbackPermissionVerbsForEntityType(item.entityType);
		const value = allowedVerbs.length > 0 ? { allowedVerbs } : undefined;
		this.#entityUserPermissionModalContext = this.#modalManagerContext?.open(this, UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
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

	#removeGranularPermission(item: UmbElementFolderItemModel) {
		const permission = this.#getPermissionForFolder(item.unique);
		if (!permission) return;

		this.permissions = this.#permissions.filter((v) => JSON.stringify(v) !== JSON.stringify(permission));
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

	#renderRef(item: UmbElementFolderItemModel) {
		if (!item.unique) return;
		const permissionNames = this.#getPermissionNamesForFolder(item.unique);

		return html`
			<uui-ref-node .name=${item.name} .detail=${permissionNames || ''}>
				${this.#renderIcon(item)}
				<uui-action-bar slot="actions">
					${this.#renderEditButton(item)} ${this.#renderRemoveButton(item)}
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbElementFolderItemModel) {
		if (!item.icon) return;
		return html`<umb-icon slot="icon" name=${item.icon}></umb-icon>`;
	}

	#renderEditButton(item: UmbElementFolderItemModel) {
		return html`
			<uui-button
				@click=${() => this.#editGranularPermission(item)}
				label=${this.localize.term('general_edit')}></uui-button>
		`;
	}

	#renderRemoveButton(item: UmbElementFolderItemModel) {
		return html`<uui-button
			@click=${() => this.#removeGranularPermission(item)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	#getPermissionForFolder(unique: string) {
		return this.#permissions?.find((permission) => permission.element.id === unique);
	}

	#getPermissionNamesForFolder(unique: string) {
		const permission = this.#getPermissionForFolder(unique);
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
		const verbs = umbExtensionsRegistry
			.getByTypeAndFilter(
				'entityUserPermission',
				(manifest) =>
					manifest.forEntityTypes.includes(entityType) &&
					this.fallbackPermissions.map((verb) => manifest.meta.verbs.includes(verb)).includes(true),
			)
			.flatMap((permission) => permission.meta.verbs);

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

export default UmbInputElementFolderGranularUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-element-folder-granular-user-permission': UmbInputElementFolderGranularUserPermissionElement;
	}
}
