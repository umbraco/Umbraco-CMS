import type { UmbUserGroupDetailModel } from '../../../types.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../components/user-group-workspace-assign-access.element.js';
import '../components/user-group-entity-type-permission-groups.element.js';
import '../components/user-group-workspace-users.element.js';

@customElement('umb-user-group-details-workspace-view')
export class UmbUserGroupDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _unique?: UmbUserGroupDetailModel['unique'];

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext?.unique, (value) => (this._unique = value ?? undefined), '_observeUnique');
		});
	}

	override render() {
		if (!this._unique) return nothing;

		return html`
			<div id="main">
				<umb-stack>
					<umb-user-group-workspace-assign-access></umb-user-group-workspace-assign-access>
					<umb-user-group-entity-type-permission-groups></umb-user-group-entity-type-permission-groups>
				</umb-stack>
				<umb-user-group-workspace-users></umb-user-group-workspace-users>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#main {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export { UmbUserGroupDetailsWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-details-workspace-view': UmbUserGroupDetailsWorkspaceViewElement;
	}
}
