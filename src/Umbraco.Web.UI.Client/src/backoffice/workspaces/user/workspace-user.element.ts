import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { css, html, LitElement, nothing, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { repeat } from 'lit/directives/repeat.js';

import { getTagLookAndColor } from '../../sections/users/user-extensions';
import { UmbUserContext } from './user.context';
import { UmbUserStore } from '@umbraco-cms/stores/user/user.store';

import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestWorkspaceAction, UserDetails } from '@umbraco-cms/models';

import '../../property-editor-uis/content-picker/property-editor-ui-content-picker.element';
import '@umbraco-cms/components/input-user-group/input-user-group.element';

import { umbCurrentUserService } from 'src/core/services/current-user';
import { UmbModalService } from '@umbraco-cms/services';
import '../shared/workspace-entity-layout/workspace-entity-layout.element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-workspace-user')
export class UmbWorkspaceUserElement extends UmbContextProviderMixin(
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
			uui-avatar {
				font-size: var(--uui-size-16);
				place-self: center;
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
			uui-tag {
				width: fit-content;
			}
			#user-info {
				display: flex;
				gap: var(--uui-size-space-6);
			}
			#user-info > div {
				display: flex;
				flex-direction: column;
			}
			#assign-access {
				display: flex;
				flex-direction: column;
			}
		`,
	];

	@state()
	private _user?: UserDetails | null;

	@state()
	private _currentUser?: UserDetails | null;

	@state()
	private _userName = '';

	@property({ type: String })
	entityKey = '';

	private _userStore?: UmbUserStore;
	private _userContext?: UmbUserContext;
	private _modalService?: UmbModalService;

	private _languages = []; //TODO Add languages

	constructor() {
		super();

		this.consumeAllContexts(['umbUserStore', 'umbModalService'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._modalService = instances['umbModalService'];

			this._observeUser();
		});

		this._observeCurrentUser();
		this._registerWorkspaceActions();
	}

	private async _observeCurrentUser() {
		if (!this._userStore) return;

		this.observe<UserDetails>(umbCurrentUserService.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private async _observeUser() {
		if (!this._userStore) return;

		this.observe<UserDetails>(this._userStore.getByKey(this.entityKey), (user) => {
			this._user = user;
			if (!this._user) return;

			if (!this._userContext) {
				this._userContext = new UmbUserContext(this._user);
				this.provideContext('umbUserContext', this._userContext);
			} else {
				this._userContext.update(this._user);
			}

			this._observeUserName();
		});
	}

	private _observeUserName() {
		if (!this._userContext) return;

		this.observe(this._userContext.data, (user) => {
			if (user.name !== this._userName) {
				this._userName = user.name;
			}
		});
	}


	private _registerWorkspaceActions() {
		const manifests: Array<ManifestWorkspaceAction> = [
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.User.Save',
				name: 'WorkspaceActionUserSave',
				loader: () => import('./actions/workspace-action-user-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.User'],
				},
			},
		];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	private _updateUserStatus() {
		if (!this._user || !this._userStore) return;

		const isDisabled = this._user.status === 'disabled';
		isDisabled ? this._userStore.enableUsers([this._user.key]) : this._userStore.disableUsers([this._user.key]);
	}

	private _deleteUser() {
		if (!this._user || !this._userStore) return;

		this._userStore.deleteUsers([this._user.key]);

		history.pushState(null, '', 'section/users/view/users/overview');
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._updateProperty('name', target.value);
			}
		}
	}

	private _updateProperty(propertyName: string, value: unknown) {
		this._userContext?.update({ [propertyName]: value });
	}

	private _renderContentStartNodes() {
		if (!this._user) return;

		if (this._user.contentStartNodes.length < 1)
			return html`
				<uui-ref-node name="Content Root">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;

		//TODO Render the name of the content start node instead of it's key.
		return repeat(
			this._user.contentStartNodes,
			(node) => node,
			(node) => {
				return html`
					<uui-ref-node name=${node}>
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			}
		);
	}

	private _changePassword() {
		this._modalService?.changePassword({ requireOldPassword: umbCurrentUserService.isAdmin === false });
	}

	private _renderActionButtons() {
		if (!this._user) return;

		const buttons: TemplateResult[] = [];

		if (umbCurrentUserService.isAdmin === false) return nothing;

		if (this._user?.status !== 'invited')
			buttons.push(
				html`
					<uui-button
						@click=${this._updateUserStatus}
						look="primary"
						color="${this._user.status === 'disabled' ? 'positive' : 'warning'}"
						label="${this._user.status === 'disabled' ? 'Enable' : 'Disable'}"></uui-button>
				`
			);

		if (this._currentUser?.key !== this._user?.key)
			buttons.push(html` <uui-button
				@click=${this._deleteUser}
				look="primary"
				color="danger"
				label="Delete User"></uui-button>`);

		buttons.push(
			html` <uui-button @click=${this._changePassword} look="primary" label="Change password"></uui-button> `
		);

		return buttons;
	}

	private _renderLeftColumn() {
		if (!this._user) return nothing;

		return html` <uui-box>
				<div slot="headline">Profile</div>
				<umb-workspace-property-layout label="Email">
					<uui-input slot="editor" name="email" label="email" readonly value=${this._user.email}></uui-input>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Language">
					<uui-select slot="editor" name="language" label="language" .options=${this._languages}> </uui-select>
				</umb-workspace-property-layout>
			</uui-box>
			<uui-box>
				<div slot="headline">Assign access</div>
				<div id="assign-access">
					<umb-workspace-property-layout label="Groups" description="Add groups to assign access and permissions">
						<umb-input-user-group
							slot="editor"
							.value=${this._user.userGroups}
							@change=${(e: any) => this._updateProperty('userGroups', e.target.value)}></umb-input-user-group>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label="Content start node"
						description="Limit the content tree to specific start nodes">
						<umb-property-editor-ui-content-picker
							.value=${this._user.contentStartNodes}
							@property-editor-change=${(e: any) => this._updateProperty('contentStartNodes', e.target.value)}
							slot="editor"></umb-property-editor-ui-content-picker>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label="Media start nodes"
						description="Limit the media library to specific start nodes">
						<b slot="editor">NEED MEDIA PICKER</b>
					</umb-workspace-property-layout>
				</div>
			</uui-box>
			<uui-box headline="Access">
				<div slot="header" class="faded-text">
					Based on the assigned groups and start nodes, the user has access to the following nodes
				</div>

				<b>Content</b>
				${this._renderContentStartNodes()}
				<hr />
				<b>Media</b>
				<uui-ref-node name="Media Root">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			</uui-box>`;
	}

	private _renderRightColumn() {
		if (!this._user || !this._userStore) return nothing;

		const statusLook = getTagLookAndColor(this._user.status);

		return html` <uui-box>
			<div id="user-info">
				<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
				<uui-button label="Change photo"></uui-button>
				<hr />
				${this._renderActionButtons()}
				<div>
					<b>Status:</b>
					<uui-tag look="${ifDefined(statusLook?.look)}" color="${ifDefined(statusLook?.color)}">
						${this._user.status}
					</uui-tag>
				</div>
				${this._user?.status === 'invited'
					? html`
							<uui-textarea placeholder="Enter a message..."> </uui-textarea>
							<uui-button look="primary" label="Resend invitation"></uui-button>
					  `
					: nothing}
				<div>
					<b>Last login:</b>
					<span>${this._user.lastLoginDate || `${this._user.name} has not logged in yet`}</span>
				</div>
				<div>
					<b>Failed login attempts</b>
					<span>${this._user.failedLoginAttempts}</span>
				</div>
				<div>
					<b>Last lockout date:</b>
					<span>${this._user.lastLockoutDate || `${this._user.name} has not been locked out`}</span>
				</div>
				<div>
					<b>Password last changed:</b>
					<span>${this._user.lastLoginDate || `${this._user.name} has not changed password`}</span>
				</div>
				<div>
					<b>User created:</b>
					<span>${this._user.createDate}</span>
				</div>
				<div>
					<b>User last updated:</b>
					<span>${this._user.updateDate}</span>
				</div>
				<div>
					<b>Key:</b>
					<span>${this._user.key}</span>
				</div>
			</div>
		</uui-box>`;
	}

	render() {
		if (!this._user) return html`User not found`;

		return html`
			<umb-workspace-entity-layout alias="Umb.Workspace.User">
				<uui-input id="name" slot="name" .value=${this._userName} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this._renderLeftColumn()}</div>
					<div id="right-column">${this._renderRightColumn()}</div>
				</div>
			</umb-workspace-entity-layout>
		`;
	}
}

export default UmbWorkspaceUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-user': UmbWorkspaceUserElement;
	}
}
