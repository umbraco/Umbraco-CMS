import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { type UmbUserPermissionSettingElement } from '@umbraco-cms/backoffice/user';
import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-user-group-default-permission-list')
export class UmbUserGroupDefaultPermissionListElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _manifests: Array<ManifestUserPermission> = [];

	@state()
	private _entityTypes: Array<string> = [];

	#userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#observeUserPermissions();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#userGroupWorkspaceContext = instance;
			this.observe(this.#userGroupWorkspaceContext.data, (userGroup) => (this._userGroup = userGroup));
		});
	}

	#observeUserPermissions() {
		this.observe(umbExtensionsRegistry.extensionsOfType('userPermission'), (userPermissionManifests) => {
			this._manifests = userPermissionManifests;
			this._entityTypes = [...new Set(userPermissionManifests.map((manifest) => manifest.meta.entityType))];
		});
	}

	#onChangeUserPermission(event: UmbChangeEvent, userPermissionManifest: ManifestUserPermission) {
		const target = event.target as UmbUserPermissionSettingElement;

		target.allowed
			? this.#userGroupWorkspaceContext?.addPermission(userPermissionManifest.alias)
			: this.#userGroupWorkspaceContext?.removePermission(userPermissionManifest.alias);
	}

	#isAllowed(userPermissionManifest: ManifestUserPermission) {
		return this._userGroup?.permissions?.includes(userPermissionManifest.alias);
	}

	render() {
		return html` ${this._entityTypes.map((entityType) => this.#renderPermissionsForEntityType(entityType))} `;
	}

	#renderPermissionsForEntityType(entityType: string) {
		return html` <h4><umb-localize .key=${`user_permissionsEntityGroup_${entityType}`}>${entityType}</umb-localize></h4>
			${this._manifests
				.filter((manifest) => manifest.meta.entityType === entityType)
				.map((manifest) => this.#renderPermission(manifest))}`;
	}

	#renderPermission(manifest: ManifestUserPermission) {
		return html` <umb-user-permission-setting
			label=${manifest.meta.label}
			description=${ifDefined(manifest.meta.description)}
			?allowed=${this.#isAllowed(manifest)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, manifest)}></umb-user-permission-setting>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupDefaultPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupDefaultPermissionListElement;
	}
}
