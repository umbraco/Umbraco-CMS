import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-user-group-entity-user-permission-list')
export class UmbUserGroupEntityUserPermissionListElement extends UmbLitElement {
	@state()
	private _userGroupPermissions?: Array<UmbUserPermissionModel>;

	@state()
	private _entityTypes: Array<string> = [];

	#userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#observeEntityUserPermissions();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#userGroupWorkspaceContext = instance;
			this.observe(
				this.#userGroupWorkspaceContext.data,
				(userGroup) => (this._userGroupPermissions = userGroup?.permissions),
				'umbUserGroupEntityUserPermissionsObserver',
			);
		});
	}

	#observeEntityUserPermissions() {
		this.observe(
			umbExtensionsRegistry.byType('entityUserPermission'),
			(manifests) => {
				this._entityTypes = [...new Set(manifests.map((manifest) => manifest.meta.entityType))];
			},
			'umbUserPermissionsObserver',
		);
	}

	#onSelectedUserPermission(event: UmbSelectionChangeEvent) {
		const target = event.target as any;
		const selection = target.selectedPermissions;
		this.#userGroupWorkspaceContext?.setPermissions(selection);
	}

	render() {
		return html` ${this._entityTypes.map((entityType) => this.#renderPermissionsByEntityType(entityType))} `;
	}

	#renderPermissionsByEntityType(entityType: string) {
		return html`
			<h4><umb-localize .key=${`user_permissionsEntityGroup_${entityType}`}>${entityType}</umb-localize></h4>
			<umb-entity-user-permission-settings-list
				.entityType=${entityType}
				.selectedPermissions=${this._userGroupPermissions || []}
				@selection-change=${this.#onSelectedUserPermission}></umb-entity-user-permission-settings-list>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupEntityUserPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupEntityUserPermissionListElement;
	}
}
