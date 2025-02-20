import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-user-group-entity-user-permission-list')
export class UmbUserGroupEntityUserPermissionListElement extends UmbLitElement {
	@state()
	private _fallBackPermissions?: Array<string>;

	@state()
	private _entityTypes: Array<string> = [];

	#userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#observeEntityUserPermissions();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#userGroupWorkspaceContext = instance;
			this.observe(
				this.#userGroupWorkspaceContext.fallbackPermissions,
				(fallbackPermissions) => {
					this._fallBackPermissions = fallbackPermissions;
				},
				'umbUserGroupEntityUserPermissionsObserver',
			);
		});
	}

	#observeEntityUserPermissions() {
		this.observe(
			umbExtensionsRegistry.byType('entityUserPermission'),
			(manifests) => {
				this._entityTypes = [...new Set(manifests.flatMap((manifest) => manifest.forEntityTypes))];
			},
			'umbUserPermissionsObserver',
		);
	}

	#onPermissionChange(event: UmbSelectionChangeEvent) {
		event.stopPropagation();
		const target = event.target as any;
		const verbs = target.allowedVerbs;
		if (verbs === undefined || verbs === null) throw new Error('The verbs are not defined');
		this.#userGroupWorkspaceContext?.setFallbackPermissions(verbs);
	}

	override render() {
		return html` ${this._entityTypes.map((entityType) => this.#renderPermissionsByEntityType(entityType))} `;
	}

	#renderPermissionsByEntityType(entityType: string) {
		return html`
			<h4><umb-localize .key=${`user_permissionsEntityGroup_${entityType}`}>${entityType}</umb-localize></h4>
			<umb-input-entity-user-permission
				.entityType=${entityType}
				.allowedVerbs=${this._fallBackPermissions || []}
				@change=${this.#onPermissionChange}></umb-input-entity-user-permission>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbUserGroupEntityUserPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupEntityUserPermissionListElement;
	}
}
