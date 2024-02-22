import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-user-group-entity-user-permission-list')
export class UmbUserGroupEntityUserPermissionListElement extends UmbLitElement {
	@state()
	private _allowedVerbs?: Array<string>;

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
				(userGroup) => {
					const allAllowedVerbs =
						userGroup?.permissions
							.filter((permission) => permission.$type === 'FallbackPermissionPresentationModel')
							.flatMap((permission) => permission.verbs) || [];

					// make a unique list of allowed verbs
					this._allowedVerbs = [...new Set(allAllowedVerbs)];
				},
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
		const selection = target.allowedVerbs;
		debugger;
		//this.#userGroupWorkspaceContext?.setPermissions(selection);
	}

	render() {
		return html` ${this._entityTypes.map((entityType) => this.#renderPermissionsByEntityType(entityType))} `;
	}

	#renderPermissionsByEntityType(entityType: string) {
		return html`
			<h4><umb-localize .key=${`user_permissionsEntityGroup_${entityType}`}>${entityType}</umb-localize></h4>
			<umb-entity-user-permission-settings-list
				.entityType=${entityType}
				.allowedVerbs=${this._allowedVerbs || []}
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
