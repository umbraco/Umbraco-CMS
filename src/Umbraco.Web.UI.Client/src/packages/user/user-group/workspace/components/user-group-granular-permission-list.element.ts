import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-group-granular-permission-list')
export class UmbUserGroupGranularPermissionListElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup));
		});
	}

	render() {
		return html`<umb-extension-slot type="userGranularPermission"></umb-extension-slot>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserGroupGranularPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-granular-permission-list': UmbUserGroupGranularPermissionListElement;
	}
}
