import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-user-group-default-permission-list')
export class UmbUserGroupDefaultPermissionListElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _userPermissionManifests: Array<ManifestUserPermission> = [];

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup));
			this.observe(
				umbExtensionsRegistry.extensionsOfType('userPermission'),
				(userPermissionManifests) => (this._userPermissionManifests = userPermissionManifests),
			);
		});
	}

	#onChangeUserPermission(event: UUIBooleanInputEvent, userPermissionManifest: ManifestUserPermission) {
		event.target.checked
			? this.#workspaceContext?.addPermission(userPermissionManifest.alias)
			: this.#workspaceContext?.removePermission(userPermissionManifest.alias);
	}

	#isAllowed(userPermissionManifest: ManifestUserPermission) {
		console.log(this._userGroup?.permissions?.includes(userPermissionManifest.alias));
		return this._userGroup?.permissions?.includes(userPermissionManifest.alias);
	}

	render() {
		return html` ${this._userPermissionManifests.map((permission) => this.#renderPermission(permission))} `;
	}

	#renderPermission(userPermissionManifest: ManifestUserPermission) {
		return html`<div style="display: flex; align-items:center; border-bottom: 1px solid whitesmoke">
			<uui-toggle
				label=${userPermissionManifest.meta.label}
				?checked=${this.#isAllowed(userPermissionManifest)}
				@change=${(event: UUIBooleanInputEvent) =>
					this.#onChangeUserPermission(event, userPermissionManifest)}></uui-toggle>
			<div>
				<h5>${userPermissionManifest.meta.label}</h5>
				<small>${userPermissionManifest.meta.description}</small>
			</div>
		</div>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserGroupDefaultPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupDefaultPermissionListElement;
	}
}
