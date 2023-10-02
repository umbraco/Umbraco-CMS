import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-user-group-default-permission-list')
export class UmbUserGroupDefaultPermissionListElement extends UmbLitElement {
	@state()
	private _userGroupDefaultPermissions?: Array<string>;

	@state()
	private _entityTypes: Array<string> = [];

	#userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#observeUserPermissions();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#userGroupWorkspaceContext = instance;
			this.observe(
				this.#userGroupWorkspaceContext.data,
				(userGroup) => (this._userGroupDefaultPermissions = userGroup?.permissions),
			);
		});
	}

	#observeUserPermissions() {
		this.observe(umbExtensionsRegistry.extensionsOfType('userPermission'), (userPermissionManifests) => {
			this._entityTypes = [...new Set(userPermissionManifests.map((manifest) => manifest.meta.entityType))];
		});
	}

	#onSelectedUserPermission(event: UmbSelectionChangeEvent) {
		const target = event.target as any;
		const selection = target.selectedPermissions;
		this.#userGroupWorkspaceContext?.setDefaultPermissions(selection);
	}

	render() {
		return html` ${this._entityTypes.map((entityType) => this.#renderPermissionsByEntityType(entityType))} `;
	}

	#renderPermissionsByEntityType(entityType: string) {
		return html`
			<h4><umb-localize .key=${`user_permissionsEntityGroup_${entityType}`}>${entityType}</umb-localize></h4>
			<umb-entity-user-permission-settings-list
				.entityType=${entityType}
				.selectedPermissions=${this._userGroupDefaultPermissions || []}
				@selection-change=${this.#onSelectedUserPermission}></umb-entity-user-permission-settings-list>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupDefaultPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupDefaultPermissionListElement;
	}
}
