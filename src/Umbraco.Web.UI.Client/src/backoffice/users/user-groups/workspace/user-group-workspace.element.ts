import { UUITextStyles } from '@umbraco-ui/uui-css';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserGroupWorkspaceContext } from './user-group-workspace.context';
import { UmbUserGroupWorkspaceEditElement } from './user-group-workspace-edit.element';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { IRoutingInfo } from '@umbraco-cms/internal/router';

@customElement('umb-user-group-workspace')
export class UmbUserGroupWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles];

	#workspaceContext = new UmbUserGroupWorkspaceContext(this);
	#element = new UmbUserGroupWorkspaceEditElement();

	constructor() {
		super();
		this._registerWorkspaceActions();
	}

	// TODO: move this to a manifest file
	private _registerWorkspaceActions() {
		const manifests: Array<ManifestWorkspaceAction> = [
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.UserGroup.Save',
				name: 'Save User Group Workspace Action',
				meta: {
					label: 'Save',
					look: 'primary',
					color: 'positive',
					api: UmbSaveWorkspaceAction,
				},
				conditions: {
					workspaces: ['Umb.Workspace.UserGroup'],
				},
			},
		];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	@state()
	_routes: any[] = [
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const key = info.match.params.key;
				this.#workspaceContext.load(key);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbUserGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace': UmbUserGroupWorkspaceElement;
	}
}
