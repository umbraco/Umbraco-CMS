import { UmbElementItemRepository } from '../item/repository/element-item.repository.js';
import { UMB_ELEMENT_PICKER_MODAL } from '../modals/element-picker-modal.token.js';
import type { UmbElementItemModel } from '../item/repository/types.js';
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

@customElement('umb-input-element-granular-user-permission')
export class UmbInputElementGranularUserPermissionElement extends UUIFormControlMixin(UmbLitElement, '') {
	#permissions: Array<UmbElementUserPermissionModel> = [];
	public get permissions(): Array<UmbElementUserPermissionModel> {
		return this.#permissions;
	}
	public set permissions(value: Array<UmbElementUserPermissionModel>) {
		this.#permissions = value;
		const uniques = value.map((item) => item.element.id);
		this.#observePickedElements(uniques);
	}

	@property({ type: Array, attribute: false })
	public fallbackPermissions: Array<string> = [];

	@state()
	private _items?: Array<UmbElementItemModel>;

	#elementItemRepository = new UmbElementItemRepository(this);
	#modalManagerContext?: UmbModalManagerContext;
	#elementPickerModalContext?: any;
	#entityUserPermissionModalContext?: any;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => (this.#modalManagerContext = instance));
	}

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedElements(uniques: Array<string>) {
		const { asObservable } = await this.#elementItemRepository.requestItems(uniques);
		this.observe(
			asObservable?.(),
			(items) => {
				this._items = items;
			},
			'observeItems',
		);
	}

	async #editGranularPermission(item: UmbElementItemModel) {
		const currentPermissionVerbs = this.#getPermissionForElement(item.unique)?.verbs ?? [];
		const result = await this.#selectEntityUserPermissionsForElement(item, currentPermissionVerbs);
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
		this.#elementPickerModalContext = this.#modalManagerContext?.open(this, UMB_ELEMENT_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				// prevent already selected items to be picked again
				pickableFilter: (treeItem: UmbElementTreeItemModel) =>
					!treeItem.isFolder && !this._items?.map((i) => i.unique).includes(treeItem.unique),
			},
		});

		this.#elementPickerModalContext?.addEventListener(UmbSelectedEvent.TYPE, async (event: UmbDeselectedEvent) => {
			const selectedEvent = event as UmbSelectedEvent;
			const unique = selectedEvent.unique;
			if (!unique) return;

			const elementItem = await this.#requestElementItem(unique);

			this.#selectEntityUserPermissionsForElement(elementItem).then(
				(result) => {
					this.#elementPickerModalContext?.reject();

					const permissionItem: UmbElementUserPermissionModel = {
						$type: 'ElementPermissionPresentationModel',
						element: { id: unique },
						verbs: result,
					};

					this.permissions = [...this.#permissions, permissionItem];
					this.dispatchEvent(new UmbChangeEvent());
				},
				() => {
					this.#elementPickerModalContext?.reject();
				},
			);
		});
	}

	async #requestElementItem(unique: string) {
		if (!unique) throw new Error('Could not open permissions modal, no unique was provided');

		const { data } = await this.#elementItemRepository.requestItems([unique]);

		const elementItem = data?.[0];
		if (!elementItem) throw new Error('No element item found');
		return elementItem;
	}

	async #selectEntityUserPermissionsForElement(item: UmbElementItemModel, allowedVerbs: Array<string> = []) {
		// TODO: get correct variant name
		const name = item.variants[0]?.name;
		const headline = name ? `Permissions for ${name}` : 'Permissions';
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

	#removeGranularPermission(item: UmbElementItemModel) {
		const permission = this.#getPermissionForElement(item.unique);
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

	#renderRef(item: UmbElementItemModel) {
		if (!item.unique) return;
		// TODO: get correct variant name
		const name = item.variants[0]?.name;
		const permissionNames = this.#getPermissionNamesForElement(item.unique);

		return html`
			<uui-ref-node .name=${name} .detail=${permissionNames || ''}>
				${this.#renderIcon(item)} ${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					${this.#renderEditButton(item)} ${this.#renderRemoveButton(item)}
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbElementItemModel) {
		if (!item.documentType.icon) return;
		return html`<umb-icon slot="icon" name=${item.documentType.icon}></umb-icon>`;
	}

	#renderIsTrashed(item: UmbElementItemModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	#renderEditButton(item: UmbElementItemModel) {
		return html`
			<uui-button
				@click=${() => this.#editGranularPermission(item)}
				label=${this.localize.term('general_edit')}></uui-button>
		`;
	}

	#renderRemoveButton(item: UmbElementItemModel) {
		return html`<uui-button
			@click=${() => this.#removeGranularPermission(item)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	#getPermissionForElement(unique: string) {
		return this.#permissions?.find((permission) => permission.element.id === unique);
	}

	#getPermissionNamesForElement(unique: string) {
		const permission = this.#getPermissionForElement(unique);
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
		// this is used to determine the default permissions for a element
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

export default UmbInputElementGranularUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-element-granular-user-permission': UmbInputElementGranularUserPermissionElement;
	}
}
