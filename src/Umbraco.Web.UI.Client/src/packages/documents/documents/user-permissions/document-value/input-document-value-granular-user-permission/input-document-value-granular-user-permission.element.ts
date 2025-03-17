import type { UmbDocumentValueUserPermissionModel } from '../types.js';
import { UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL } from '../document-value-granular-permission-flow-modal/index.js';
import { css, customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestEntityUserPermission } from '@umbraco-cms/backoffice/user-permission';
import { UmbDocumentTypeItemRepository, type UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

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

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedDocumentTypes(uniques: Array<string>) {
		const { asObservable } = await this.#documentTypeItemRepository.requestItems(uniques);
		this.observe(asObservable(), (items) => (this._items = items));
	}

	async #addGranularPermission() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Could not open modal, no modal manager found');
		}

		const modal = modalManager.open(this, UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL, {
			data: {
				preset: {
					verbs: this.#getFallbackPermissionVerbsForEntityType('document-value'),
				},
			},
		});

		try {
			const value = await modal?.onSubmit();
			if (!value) throw new Error('No result from modal');

			const permissionItem: UmbDocumentValueUserPermissionModel = {
				$type: 'DocumentValuePermissionPresentationModel',
				documentType: value.documentType,
				propertyType: value.propertyType,
				verbs: value.verbs,
			};

			this.permissions = [...this._permissions, permissionItem];
			this.dispatchEvent(new UmbChangeEvent());
		} catch (error) {
			console.error(error);
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
