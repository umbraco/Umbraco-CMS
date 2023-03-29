import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { css, html, nothing, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { repeat } from 'lit/directives/repeat.js';

import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../../current-user/current-user.store';
import { UmbUserWorkspaceContext } from './user-workspace.context';
import { UMB_CHANGE_PASSWORD_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { getLookAndColorFromUserStatus } from '@umbraco-cms/backoffice/utils';
import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../../../shared/components/input-user-group/input-user-group.element';
import '../../../shared/property-editors/uis/document-picker/property-editor-ui-document-picker.element';
import '../../../shared/components/workspace/workspace-layout/workspace-layout.element';

@customElement('umb-user-workspace-edit')
export class UmbUserWorkspaceEditElement extends UmbLitElement {
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
	private _currentUser?: UserDetails;

	private _currentUserStore?: UmbCurrentUserStore;
	private _modalContext?: UmbModalContext;

	private _languages = []; //TODO Add languages

	private _workspaceContext: UmbUserWorkspaceContext = new UmbUserWorkspaceContext(this);

	@state()
	private _user?: UserDetails;

	@state()
	private _userName = '';

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (store) => {
			this._currentUserStore = store;
			this._observeCurrentUser();
		});

		this.observe(this._workspaceContext.data, (user) => {
			// TODO: fix type mismatch:
			this._user = user as any;
			if (user && user.name !== this._userName) {
				this._userName = user.name || '';
			}
		});
	}

	private async _observeCurrentUser() {
		if (!this._currentUserStore) return;

		// TODO: do not have static current user service, we need to make a ContextAPI for this.
		this.observe(this._currentUserStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _updateUserStatus() {
		if (!this._user || !this._workspaceContext) return;

		const isDisabled = this._user.status === 'disabled';
		// TODO: make sure we use the workspace for this:
		/*
		isDisabled
			? this._workspaceContext.getStore()?.enableUsers([this._user.key])
			: this._workspaceContext.getStore()?.disableUsers([this._user.key]);
			*/
	}

	private _deleteUser() {
		if (!this._user || !this._workspaceContext) return;

		// TODO: make sure we use the workspace for this:
		//this._workspaceContext.getStore()?.deleteUsers([this._user.key]);

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
		this._workspaceContext?.update({ [propertyName]: value });
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
		this._modalContext?.open(UMB_CHANGE_PASSWORD_MODAL, {
			requireOldPassword: this._currentUserStore?.isAdmin === false,
		});
	}

	private _renderActionButtons() {
		if (!this._user) return;

		const buttons: TemplateResult[] = [];

		if (this._currentUserStore?.isAdmin === false) return nothing;

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
						<umb-property-editor-ui-document-picker
							.value=${this._user.contentStartNodes}
							@property-editor-change=${(e: any) => this._updateProperty('contentStartNodes', e.target.value)}
							slot="editor"></umb-property-editor-ui-document-picker>
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
		if (!this._user || !this._workspaceContext) return nothing;

		const statusLook = getLookAndColorFromUserStatus(this._user.status);

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
			<umb-workspace-layout alias="Umb.Workspace.User">
				<uui-input id="name" slot="name" .value=${this._userName} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this._renderLeftColumn()}</div>
					<div id="right-column">${this._renderRightColumn()}</div>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbUserWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-edit': UmbUserWorkspaceEditElement;
	}
}
