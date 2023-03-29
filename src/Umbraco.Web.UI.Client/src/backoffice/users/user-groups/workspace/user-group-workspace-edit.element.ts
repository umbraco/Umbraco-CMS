import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbUserStore } from '../../users/repository/user.store';
import { UmbUserGroupWorkspaceContext } from './user-group-workspace.context';
import type { UserGroupDetails } from '@umbraco-cms/backoffice/models';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../../../shared/components/input-user/input-user.element';
import '../../../shared/components/input-section/input-section.element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-user-group-workspace-edit')
export class UmbUserGroupWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
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
			#left-column {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#right-column > uui-box > div {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
			#default-permissions {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.default-permission {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-2);
			}
			.default-permission:not(:last-child) {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			.permission-info {
				display: flex;
				flex-direction: column;
			}
		`,
	];

	defaultPermissions: Array<{
		name: string;
		permissions: Array<{ name: string; description: string; value: boolean }>;
	}> = [
		{
			name: 'Administration',
			permissions: [
				{
					name: 'Culture and Hostnames',
					description: 'Allow access to assign culture and hostnames',
					value: false,
				},
				{
					name: 'Restrict Public Access',
					description: 'Allow access to set and change access restrictions for a node',
					value: false,
				},
				{
					name: 'Rollback',
					description: 'Allow access to roll back a node to a previous state',
					value: false,
				},
			],
		},
		{
			name: 'Content',
			permissions: [
				{
					name: 'Browse Node',
					description: 'Allow access to view a node',
					value: false,
				},
				{
					name: 'Create Content Template',
					description: 'Allow access to create a Content Template',
					value: false,
				},
				{
					name: 'Delete',
					description: 'Allow access to delete nodes',
					value: false,
				},
				{
					name: 'Create',
					description: 'Allow access to create nodes',
					value: false,
				},
				{
					name: 'Publish',
					description: 'Allow access to publish nodes',
					value: false,
				},
				{
					name: 'Permissions',
					description: 'Allow access to change permissions for a node',
					value: false,
				},
				{
					name: 'Send To Publish',
					description: 'Allow access to send a node for approval before publishing',
					value: false,
				},
				{
					name: 'Unpublish',
					description: 'Allow access to unpublish a node',
					value: false,
				},
				{
					name: 'Update',
					description: 'Allow access to save a node',
					value: false,
				},
				{
					name: 'Full restore',
					description: 'Allow the user to restore items',
					value: false,
				},
				{
					name: 'Partial restore',
					description: 'Allow the user to partial restore items',
					value: false,
				},
				{
					name: 'Queue for transfer',
					description: 'Allow the user to queue item(s)',
					value: false,
				},
			],
		},
		{
			name: 'Structure',
			permissions: [
				{
					name: 'Copy',
					description: 'Allow access to copy a node',
					value: false,
				},
				{
					name: 'Move',
					description: 'Allow access to move a node',
					value: false,
				},
				{
					name: 'Sort',
					description: 'Allow access to change the sort order for nodes',
					value: false,
				},
			],
		},
	];

	private _userStore?: UmbUserStore;

	#workspaceContext?: UmbUserGroupWorkspaceContext;

	@state()
	private _userGroup?: UserGroupDetails;

	@state()
	private _userKeys?: Array<string>;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbUserGroupWorkspaceContext;
			this.observeUserGroup();
		});
	}

	observeUserGroup() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup as any));
	}

	private _observeUsers() {
		if (!this._userStore) return;

		// TODO: Create method to only get users from this userGroup
		// TODO: Find a better way to only call this once at the start
		this.observe(this._userStore.getAll(), (users) => {
			// TODO: handle if there is no users.
			if (!this._userKeys && users.length > 0) {
				const entityKey = this.#workspaceContext?.getEntityKey();
				if (!entityKey) return;
				this._userKeys = users.filter((user) => user.userGroups.includes(entityKey)).map((user) => user.key);
				//this._updateProperty('users', this._userKeys);
				// TODO: make a method on the UmbWorkspaceUserGroupContext:
				//this._workspaceContext.setUsers();
			}
		});
	}

	private _updateUserKeys(userKeys: Array<string>) {
		this._userKeys = userKeys;
		// TODO: make a method on the UmbWorkspaceUserGroupContext:
		//this._workspaceContext.setUsers();
	}

	private _updatePermission(permission: { name: string; description: string; value: boolean }) {
		if (!this.#workspaceContext) return;

		const checkValue = this._checkPermission(permission);
		//const selectedPermissions = this._workspaceContext.getData().permissions;
		// TODO: make a method on the UmbWorkspaceUserGroupContext:
		//const selectedPermissions = this._workspaceContext.getPermissions();

		/*
		let newPermissions = [];
		if (checkValue === false) {
			newPermissions = [...selectedPermissions, permission.name];
		} else {
			newPermissions = selectedPermissions.filter((p) => p !== permission.name);
		}
		*/

		//this._updateProperty('permissions', newPermissions);
		// TODO: make a method on the UmbWorkspaceUserGroupContext:
		//this._workspaceContext.setPermissions();
	}

	private _checkPermission(permission: { name: string; description: string; value: boolean }) {
		if (!this.#workspaceContext) return false;

		//return this._workspaceContext.getPermissions().includes(permission.name);
		return false;
	}

	private _updateSections(value: string[]) {
		console.log('To be done');
		//this._workspaceContext.setSections(value);
	}

	private renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<umb-workspace-property-layout label="Sections" description="Add sections to give users access">
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections}
						@change=${(e: any) => this._updateSections(e.target.value)}></umb-input-section>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Content start node"
					description="Limit the content tree to a specific start node">
					<uui-ref-node slot="editor" name="Content Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Media start node"
					description="Limit the media library to a specific start node">
					<uui-ref-node slot="editor" name="Media Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-workspace-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline">Default Permissions</div>
				<div id="default-permissions">
					${repeat(
						this.defaultPermissions,
						(defaultPermission) => html`
							<div>
								<b>${defaultPermission.name}</b>
								${repeat(
									defaultPermission.permissions,
									(permission) => html`
										<div class="default-permission">
											<uui-toggle
												.checked=${this._checkPermission(permission)}
												@change=${() => this._updatePermission(permission)}></uui-toggle>
											<div class="permission-info">
												<b>${permission.name}</b>
												<span class="faded-text">${permission.description}</span>
											</div>
										</div>
									`
								)}
							</div>
						`
					)}
				</div>
			</uui-box>

			<uui-box>
				<div slot="headline">Granular permissions</div>
			</uui-box>`;
	}

	private renderRightColumn() {
		return html`<uui-box>
			<div slot="headline">Users</div>
			<umb-input-user
				@change=${(e: Event) => this._updateUserKeys((e.target as any).value)}
				.value=${this._userKeys || []}></umb-input-user>
		</uui-box>`;
	}

	// TODO: find a way where we don't have to do this for all Workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		if (!this._userGroup) return nothing;

		return html`
			<umb-workspace-layout alias="Umb.Workspace.UserGroup">
				<uui-input id="name" slot="header" .value=${this._userGroup.name} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this.renderLeftColumn()}</div>
					<div id="right-column">${this.renderRightColumn()}</div>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbUserGroupWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-edit': UmbUserGroupWorkspaceEditElement;
	}
}
