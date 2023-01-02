import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceUserGroupContext } from './workspace-user-group.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import '@umbraco-cms/components/input-user/input-user.element';
import '@umbraco-cms/components/input-section/input-section.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestWorkspaceAction, UserDetails, UserGroupDetails } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbUserStore } from '@umbraco-cms/stores/user/user.store';

@customElement('umb-workspace-user-group')
export class UmbWorkspaceUserGroupElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
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
				gap: var(--uui-size-space-6);
				padding: var(--uui-size-space-6);
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

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		this._provideWorkspace();
	}

	private _workspaceContext?:UmbWorkspaceUserGroupContext;


	@state()
	private _userGroup?: UserGroupDetails | null;

	@state()
	private _userKeys?: Array<string>;

	constructor() {
		super();

		this._registerWorkspaceActions();

		this.consumeContext('umbUserStore', (instance) => {
			this._userStore = instance;
			this._observeUsers();
		});

	}


	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if(this._entityKey) {
			this._workspaceContext = new UmbWorkspaceUserGroupContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);

			this._observeUserGroup();
		}
	}

	private _registerWorkspaceActions() {
		const manifests: Array<ManifestWorkspaceAction> = [
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.UserGroup.Save',
				name: 'Save User Group Workspace Action',
				loader: () => import('../shared/actions/save/workspace-action-node-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.UserGroup'],
					look: 'primary',
					color: 'positive'
				},
			},
		];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	private _observeUserGroup() {
		if (!this._workspaceContext) return;

		this.observe<UserGroupDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (userGroup) => {
			if (!this._userGroup) return;
			this._userGroup = userGroup;
		});
	}

	private _observeUsers() {
		if (!this._userStore) return;

		// TODO: Create method to only get users from this userGroup
		// TODO: Find a better way to only call this once at the start
		this.observe(this._userStore.getAll(), (users: Array<UserDetails>) => {
			if (!this._userKeys && users.length > 0) {
				this._userKeys = users.filter((user) => user.userGroups.includes(this.entityKey)).map((user) => user.key);
				this._updateProperty('users', this._userKeys);
			}
		});
	}

	private _updateUserKeys(userKeys: Array<string>) {
		this._userKeys = userKeys;
		this._updateProperty('users', this._userKeys);
	}

	private _updateProperty(propertyName: string, value: unknown) {
		this._workspaceContext?.update({ [propertyName]: value });
	}

	private _updatePermission(permission: { name: string; description: string; value: boolean }) {
		if (!this._workspaceContext) return;

		const checkValue = this._checkPermission(permission);
		const selectedPermissions = this._workspaceContext.getData().permissions;

		let newPermissions = [];
		if (checkValue === false) {
			newPermissions = [...selectedPermissions, permission.name];
		} else {
			newPermissions = selectedPermissions.filter((p) => p !== permission.name);
		}
		this._updateProperty('permissions', newPermissions);
	}

	private _checkPermission(permission: { name: string; description: string; value: boolean }) {
		if (!this._workspaceContext) return false;

		return this._workspaceContext.getData().permissions.includes(permission.name);
	}

	private renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<umb-workspace-property-layout label="Sections" description="Add sections to give users access">
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections}
						@change=${(e: any) => this._updateProperty('sections', e.target.value)}></umb-input-section>
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
				this._updateProperty('name', target.value);
			}
		}
	}

	render() {
		if (!this._userGroup) return nothing;

		return html`
			<umb-workspace-entity alias="Umb.Workspace.UserGroup">
				<uui-input id="name" slot="header" .value=${this._userGroup.name} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this.renderLeftColumn()}</div>
					<div id="right-column">${this.renderRightColumn()}</div>
				</div>
			</umb-workspace-entity>
		`;
	}
}

export default UmbWorkspaceUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-user-group': UmbWorkspaceUserGroupElement;
	}
}
