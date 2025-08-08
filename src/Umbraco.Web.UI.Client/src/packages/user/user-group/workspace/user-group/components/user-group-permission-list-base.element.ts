import { html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

export abstract class UmbUserGroupPermissionsListBaseElement extends UmbLitElement {
	protected userGroupWorkspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	@state()
	protected _fallBackPermissions?: Array<string>;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.userGroupWorkspaceContext = instance;
			this.observe(
				this.userGroupWorkspaceContext?.fallbackPermissions,
				(fallbackPermissions) => {
					this._fallBackPermissions = fallbackPermissions;
				},
				'umbUserGroupFallbackPermissionsObserver',
			);
		});
	}

	protected renderPermissionsForEntityType(group: { entityType: string; headline: string }) {
		return html`
			<h4>${group.headline}</h4>
			<umb-input-entity-user-permission
				.entityType=${group.entityType}
				.allowedVerbs=${this._fallBackPermissions || []}
				@change=${this.onPermissionChange}></umb-input-entity-user-permission>
		`;
	}

	protected onPermissionChange(event: UmbSelectionChangeEvent) {
		event.stopPropagation();
		const target = event.target as any;
		const verbs = target.allowedVerbs;

		if (verbs === undefined || verbs === null) throw new Error('The verbs are not defined');

		this.userGroupWorkspaceContext?.setFallbackPermissions(verbs);
	}

	static override styles = [UmbTextStyles];
}
