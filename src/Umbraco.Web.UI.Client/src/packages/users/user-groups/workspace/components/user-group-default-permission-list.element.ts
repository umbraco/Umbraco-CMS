import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';

@customElement('umb-user-group-default-permission-list')
export class UmbUserGroupDefaultPermissionListElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _groupedUserPermissionManifests: Record<string, Array<ManifestUserPermission>> = {};

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#observeUserPermissions();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup));
		});
	}

	#observeUserPermissions() {
		this.observe(umbExtensionsRegistry.extensionsOfType('userPermission'), (userPermissionManifests) => {
			this._groupedUserPermissionManifests = groupBy(userPermissionManifests, (manifest) => manifest.meta.entityType);
		});
	}

	#onChangeUserPermission(event: UUIBooleanInputEvent, userPermissionManifest: ManifestUserPermission) {
		event.target.checked
			? this.#workspaceContext?.addPermission(userPermissionManifest.alias)
			: this.#workspaceContext?.removePermission(userPermissionManifest.alias);
	}

	#isAllowed(userPermissionManifest: ManifestUserPermission) {
		return this._userGroup?.permissions?.includes(userPermissionManifest.alias);
	}

	render() {
		const entityGroups = [];

		for (const [key, value] of Object.entries(this._groupedUserPermissionManifests)) {
			entityGroups.push(
				html`<h3><umb-localize .key=${`user_permissionsEntityGroup_${key}`}>${key}</umb-localize></h3>
					${value.map((permission) => this.#renderPermission(permission))}`,
			);
		}

		return html`${entityGroups}`;
	}

	#renderPermission(userPermissionManifest: ManifestUserPermission) {
		return html`<div
			style="display: flex; align-items:center; border-bottom: 1px solid var(--uui-color-divider); padding: 9px 0 12px 0;">
			<uui-toggle
				label=${userPermissionManifest.meta.label}
				?checked=${this.#isAllowed(userPermissionManifest)}
				@change=${(event: UUIBooleanInputEvent) => this.#onChangeUserPermission(event, userPermissionManifest)}>
				<div class="permission-meta">
					<div class="permission-name">${userPermissionManifest.meta.label}</div>
					<small>${userPermissionManifest.meta.description}</small>
				</div>
			</uui-toggle>
		</div>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.permission-toggle {
				display: flex;
				align-items: center;
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-space-3) 0 var(--uui-size-space-4) 0;
			}

			.permission-meta {
				margin-left: var(--uui-size-space-4);
				line-height: 1.2em;
			}

			.permission-name {
				font-weight: bold;
			}
		`,
	];
}

export default UmbUserGroupDefaultPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupDefaultPermissionListElement;
	}
}
