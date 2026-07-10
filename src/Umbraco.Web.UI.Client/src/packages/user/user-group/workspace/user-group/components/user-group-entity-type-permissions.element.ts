import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-user-group-entity-type-permissions')
export class UmbUserGroupEntityTypePermissionsElement extends UmbLitElement {
	@property()
	public entityType?: string;

	@state()
	private _fallBackPermissions?: Array<string>;

	#userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#userGroupWorkspaceContext = instance;
			this.observe(
				this.#userGroupWorkspaceContext?.fallbackPermissions,
				(fallbackPermissions) => {
					this._fallBackPermissions = fallbackPermissions;
				},
				'umbUserGroupEntityUserPermissionsObserver',
			);
		});
	}

	#onPermissionChange(event: UmbSelectionChangeEvent) {
		event.stopPropagation();
		const target = event.target as any;
		const verbs = target.allowedVerbs;
		if (verbs === undefined || verbs === null) throw new Error('The verbs are not defined');
		this.#userGroupWorkspaceContext?.setFallbackPermissions(verbs);
	}

	override render() {
		if (!this.entityType) return nothing;
		return html`
			<umb-input-entity-user-permission
				.entityType=${this.entityType}
				.allowedVerbs=${this._fallBackPermissions || []}
				@change=${this.#onPermissionChange}></umb-input-entity-user-permission>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbUserGroupEntityTypePermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-entity-type-permissions': UmbUserGroupEntityTypePermissionsElement;
	}
}
