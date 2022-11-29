import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbUserGroupContext } from './user-group.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import '@umbraco-cms/sections/users/picker-user.element';
import '@umbraco-cms/sections/users/picker-section.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestEditorAction, ManifestWithLoader, UserDetails, UserGroupDetails } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbUserGroupStore } from '@umbraco-cms/stores/user/user-group.store';
import { UmbUserStore } from '@umbraco-cms/stores/user/user.store';

@customElement('umb-editor-user-group')
export class UmbEditorUserGroupElement extends UmbContextProviderMixin(
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

	@property({ type: String })
	entityKey = '';

	@state()
	private _userGroup?: UserGroupDetails | null;

	@state()
	private _userKeys?: Array<string>;

	private _userGroupStore?: UmbUserGroupStore;
	private _userStore?: UmbUserStore;
	private _userGroupContext?: UmbUserGroupContext;

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

	constructor() {
		super();

		this._registerEditorActions();
	}

	private _registerEditorActions() {
		const manifests: Array<ManifestWithLoader<ManifestEditorAction>> = [
			{
				type: 'editorAction',
				alias: 'Umb.EditorAction.UserGroup.Save',
				name: 'EditorActionUserGroupSave',
				loader: () => import('./actions/editor-action-user-group-save.element'),
				meta: {
					editors: ['Umb.Editor.UserGroup'],
				},
			},
		];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeAllContexts(['umbUserGroupStore', 'umbUserStore'], (instance) => {
			this._userGroupStore = instance['umbUserGroupStore'];
			this._userStore = instance['umbUserStore'];
			this._observeUserGroup();
			this._observeUsers();
		});
	}

	private _observeUserGroup() {
		if (!this._userGroupStore) return;

		this.observe(this._userGroupStore.getByKey(this.entityKey), (userGroup) => {
			this._userGroup = userGroup;
			if (!this._userGroup) return;

			if (!this._userGroupContext) {
				this._userGroupContext = new UmbUserGroupContext(this._userGroup);
				this.provideContext('umbUserGroupContext', this._userGroupContext);
			} else {
				this._userGroupContext.update(this._userGroup);
			}
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
		this._userGroupContext?.update({ [propertyName]: value });
	}

	private _updatePermission(permission: { name: string; description: string; value: boolean }) {
		if (!this._userGroupContext) return;

		const checkValue = this._checkPermission(permission);
		const selectedPermissions = this._userGroupContext.getData().permissions;

		let newPermissions = [];
		if (checkValue === false) {
			newPermissions = [...selectedPermissions, permission.name];
		} else {
			newPermissions = selectedPermissions.filter((p) => p !== permission.name);
		}
		this._updateProperty('permissions', newPermissions);
	}

	private _checkPermission(permission: { name: string; description: string; value: boolean }) {
		if (!this._userGroupContext) return false;

		return this._userGroupContext.getData().permissions.includes(permission.name);
	}

	private renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<umb-editor-property-layout label="Sections" description="Add sections to give users access">
					<umb-picker-section
						slot="editor"
						.value=${this._userGroup.sections}
						@change=${(e: any) => this._updateProperty('sections', e.target.value)}></umb-picker-section>
				</umb-editor-property-layout>
				<umb-editor-property-layout
					label="Content start node"
					description="Limit the content tree to a specific start node">
					<uui-ref-node slot="editor" name="Content Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-editor-property-layout>
				<umb-editor-property-layout
					label="Media start node"
					description="Limit the media library to a specific start node">
					<uui-ref-node slot="editor" name="Media Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-editor-property-layout>
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
			<umb-picker-user
				@change=${(e: Event) => this._updateUserKeys((e.target as any).value)}
				.value=${this._userKeys || []}></umb-picker-user>
		</uui-box>`;
	}

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;
		}
	}

	render() {
		if (!this._userGroup) return nothing;

		return html`
			<umb-editor-entity-layout alias="Umb.Editor.UserGroup">
				<uui-input id="name" slot="name" .value=${this._userGroup.name} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this.renderLeftColumn()}</div>
					<div id="right-column">${this.renderRightColumn()}</div>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-user-group': UmbEditorUserGroupElement;
	}
}
