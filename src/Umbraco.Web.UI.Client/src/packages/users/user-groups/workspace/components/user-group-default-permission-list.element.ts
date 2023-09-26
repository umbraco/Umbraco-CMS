import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { type UmbUserPermissionSettingElement } from '@umbraco-cms/backoffice/users';

@customElement('umb-user-group-default-permission-list')
export class UmbUserGroupDefaultPermissionListElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _groupedUserPermissionManifests: Record<string, Array<ManifestUserPermission>> = {};

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
			this._groupedUserPermissionManifests = groupBy(userPermissionManifests, (manifest) => manifest.meta.entityType);
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
		return html`${this.#renderEntityGroups()}`;
	}

	#renderEntityGroups() {
		const entityGroups = [];

		for (const [key, value] of Object.entries(this._groupedUserPermissionManifests)) {
			entityGroups.push(
				html`<h5><umb-localize .key=${`user_permissionsEntityGroup_${key}`}>${key}</umb-localize></h5>
					${value.map((permission) => this.#renderPermission(permission))}`,
			);
		}

		return html`${entityGroups}`;
	}

	#renderPermission(userPermissionManifest: ManifestUserPermission) {
		return html` <umb-user-permission-setting
			label=${userPermissionManifest.meta.label}
			description=${ifDefined(userPermissionManifest.meta.description)}
			?allowed=${this.#isAllowed(userPermissionManifest)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, userPermissionManifest)}></umb-user-permission-setting>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupDefaultPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupDefaultPermissionListElement;
	}
}
