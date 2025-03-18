import type { UmbDocumentPropertyValueUserPermissionModel as UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL } from '../document-property-value-granular-permission-flow-modal/index.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE } from '../user-permission.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE } from '../../../entity.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_ENTITY_USER_PERMISSION_MODAL,
	type ManifestEntityUserPermission,
} from '@umbraco-cms/backoffice/user-permission';
import {
	UmbDocumentTypeDetailRepository,
	type UmbDocumentTypeDetailModel,
} from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-document-property-value-granular-user-permission')
export class UmbInputDocumentPropertyValueGranularUserPermissionElement extends UUIFormControlMixin(UmbLitElement, '') {
	_permissions: Array<UmbDocumentPropertyValueUserPermissionModel> = [];
	public get permissions(): Array<UmbDocumentPropertyValueUserPermissionModel> {
		return this._permissions;
	}
	public set permissions(value: Array<UmbDocumentPropertyValueUserPermissionModel>) {
		this._permissions = value;
		const uniques = value.map((item) => item.documentType.unique);
		this.#observePickedDocumentTypes(uniques);
	}

	@property({ type: Array, attribute: false })
	fallbackPermissions: Array<string> = [];

	@state()
	private _documentTypes?: Array<UmbDocumentTypeDetailModel>;

	#documentTypeDetailRepository = new UmbDocumentTypeDetailRepository(this);

	protected override getFormElement() {
		return undefined;
	}

	async #observePickedDocumentTypes(uniques: Array<string>) {
		const promises = uniques.map((unique) => this.#documentTypeDetailRepository.requestByUnique(unique));
		const responses = await Promise.allSettled(promises);

		// TODO: handle errors
		this._documentTypes = responses
			.filter((response) => response.status === 'fulfilled')
			.map((response) => response.value.data)
			.filter((item) => item) as Array<UmbDocumentTypeDetailModel>;
	}

	async #addGranularPermission() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Could not open modal, no modal manager found');
		}

		const modal = modalManager.open(this, UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL, {
			data: {
				preset: {
					verbs: this.#getFallbackPermissionVerbsForEntityType(UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE),
				},
			},
		});

		try {
			const value = await modal?.onSubmit();
			if (!value) throw new Error('No result from modal');

			const permissionItem: UmbDocumentPropertyValueUserPermissionModel = {
				$type: 'DocumentTypePropertyPermissionPresentationModel',
				userPermissionType: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE,
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

	async #editGranularPermission(currentPermission: UmbDocumentPropertyValueUserPermissionModel) {
		if (!currentPermission) {
			throw new Error('Could not open permissions modal, no item was provided');
		}

		const documentType = this._documentTypes?.find((item) => item.unique === currentPermission.documentType.unique);

		if (!documentType) {
			throw new Error('Could not open permissions modal, no document type was found');
		}

		// TODO: show document type and property type name
		const headline = `Permissions`;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Could not open permissions modal, modal manager is not available');
		}

		const modal = modalManager.open(this, UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
				entityType: UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE,
				headline,
				preset: {
					allowedVerbs: currentPermission.verbs,
				},
			},
		});

		try {
			const value = await modal.onSubmit();

			// don't do anything if the verbs have not been updated
			if (JSON.stringify(value.allowedVerbs) === JSON.stringify(currentPermission.verbs)) return;

			// update permission with new verbs
			this.permissions = this._permissions.map((permission) => {
				if (permission.propertyType.unique === currentPermission.propertyType.unique) {
					return {
						...permission,
						verbs: value.allowedVerbs,
					};
				}
				return permission;
			});

			this.dispatchEvent(new UmbChangeEvent());
		} catch (error) {
			console.log(error);
		}
	}

	#removeGranularPermission(permission: UmbDocumentPropertyValueUserPermissionModel) {
		this.permissions = this._permissions.filter((v) => JSON.stringify(v) !== JSON.stringify(permission));
		this.dispatchEvent(new UmbChangeEvent());
	}

	#getVerbNamesForPermission(permission: UmbDocumentPropertyValueUserPermissionModel) {
		if (!permission) {
			throw new Error('Could not find permission for property type');
		}

		if (permission.verbs.length === 0) {
			return this.localize.term('user_permissionNoVerbs');
		}

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

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderItems() {
		if (!this.permissions) return;
		return html`
			<uui-ref-list>
				${repeat(
					this.permissions,
					(item) => item.propertyType.unique,
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

	#renderRef(permission: UmbDocumentPropertyValueUserPermissionModel) {
		if (!permission.propertyType.unique) {
			throw new Error('Property type unique is required');
		}

		const documentType = this._documentTypes?.find((item) => item.unique === permission.documentType.unique);
		const propertyType = documentType?.properties.find((item) => item.unique === permission.propertyType.unique);
		const permissionName = `${documentType?.name}: ${propertyType?.name}`;
		const verbNames = this.#getVerbNamesForPermission(permission);

		return html`
			<uui-ref-node .name=${permissionName} .detail=${verbNames || ''} readonly>
				${documentType?.icon ? html`<uui-icon slot="icon" name=${ifDefined(documentType?.icon)}></uui-icon>` : nothing}
				<uui-action-bar slot="actions"
					>${this.#renderEditButton(permission)} ${this.#renderRemoveButton(permission)}</uui-action-bar
				>
			</uui-ref-node>
		`;
	}

	#renderEditButton(permission: UmbDocumentPropertyValueUserPermissionModel) {
		return html`<uui-button
			@click=${() => this.#editGranularPermission(permission)}
			label=${this.localize.term('general_edit')}></uui-button>`;
	}

	#renderRemoveButton(permission: UmbDocumentPropertyValueUserPermissionModel) {
		return html`<uui-button
			@click=${() => this.#removeGranularPermission(permission)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export { UmbInputDocumentPropertyValueGranularUserPermissionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-property-value-granular-user-permission': UmbInputDocumentPropertyValueGranularUserPermissionElement;
	}
}
