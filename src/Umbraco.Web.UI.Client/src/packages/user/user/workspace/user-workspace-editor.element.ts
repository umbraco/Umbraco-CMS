import { getDisplayStateFromUserStatus } from '../../utils.js';
import { type UmbUserDetail } from '../index.js';
import { UmbUserWorkspaceContext } from './user-workspace.context.js';
import { type UmbUserGroupInputElement } from '@umbraco-cms/backoffice/user-group';
import { UmbUserRepository } from '@umbraco-cms/backoffice/user';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	nothing,
	TemplateResult,
	customElement,
	state,
	ifDefined,
	repeat,
} from '@umbraco-cms/backoffice/external/lit';
import { UMB_CHANGE_PASSWORD_MODAL, type UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';

// Import of local components that should only be used here
import './components/user-workspace-profile-settings/user-workspace-profile-settings.element.js';
import './components/user-workspace-access-settings/user-workspace-access-settings.element.js';

@customElement('umb-user-workspace-editor')
export class UmbUserWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbLoggedInUser;

	@state()
	private _user?: UmbUserDetail;

	#modalContext?: UmbModalManagerContext;
	#workspaceContext?: UmbUserWorkspaceContext;

	#userRepository = new UmbUserRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext as UmbUserWorkspaceContext;
			this.#observeUser();
		});
	}

	#observeUser() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (user) => (this._user = user));
	}

	#onUserStatusChange() {
		if (!this._user || !this._user.id) return;

		if (this._user.state === UserStateModel.ACTIVE || this._user.state === UserStateModel.INACTIVE) {
			this.#userRepository?.disable([this._user.id]);
		}

		if (this._user.state === UserStateModel.DISABLED) {
			this.#userRepository?.enable([this._user.id]);
		}
	}

	#onUserDelete() {
		if (!this._user || !this._user.id) return;

		this.#userRepository?.delete(this._user.id);
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.updateProperty('name', target.value);
			}
		}
	}

	#onPasswordChange() {
		// TODO: check if current user is admin
		this.#modalContext?.open(UMB_CHANGE_PASSWORD_MODAL, {
			requireOldPassword: false,
		});
	}

	render() {
		if (!this._user) return html`User not found`;

		return html`
			<umb-workspace-editor alias="Umb.Workspace.User" class="uui-text">
				${this.#renderHeader()}
				<div id="main">
					<div id="left-column">${this.#renderLeftColumn()}</div>
					<div id="right-column">${this.#renderRightColumn()}</div>
				</div>
			</umb-workspace-editor>
		`;
	}

	#renderHeader() {
		return html`
			<div id="header" slot="header">
				<a href="/section/users">
					<uui-icon name="umb:arrow-left"></uui-icon>
				</a>
				<uui-input id="name" .value=${this._user?.name ?? ''} @input="${this.#onNameChange}"></uui-input>
			</div>
		`;
	}

	#renderLeftColumn() {
		if (!this._user) return nothing;

		return html`
			<umb-user-workspace-profile-settings></umb-user-workspace-profile-settings
			><umb-user-workspace-access-settings></umb-user-workspace-access-settings>
		`;
	}

	#renderRightColumn() {
		if (!this._user || !this.#workspaceContext) return nothing;

		const displayState = getDisplayStateFromUserStatus(this._user.state);

		return html` <uui-box>
			<div id="user-info">
				<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
				<uui-button label=${this.localize.term('user_changePhoto')}></uui-button>
				<hr />
				${this.#renderActionButtons()}

				<div>
					<b><umb-localize key="general_status">Status</umb-localize>:</b>
					<uui-tag look="${ifDefined(displayState?.look)}" color="${ifDefined(displayState?.color)}">
						${this.localize.term('user_' + displayState.key)}
					</uui-tag>
				</div>

				${this._user?.state === UserStateModel.INVITED
					? html`
							<uui-textarea placeholder=${this.localize.term('placeholders_enterMessage')}></uui-textarea>
							<uui-button look="primary" label=${this.localize.term('actions_resendInvite')}></uui-button>
					  `
					: nothing}
				${this.#renderInfoItem(
					'user_lastLogin',
					this.localize.date(this._user.lastLoginDate!) ||
						`${this._user.name + ' ' + this.localize.term('user_noLogin')} `,
				)}
				${this.#renderInfoItem('user_failedPasswordAttempts', this._user.failedLoginAttempts)}
				${this.#renderInfoItem(
					'user_lastLockoutDate',
					this._user.lastLockoutDate || `${this._user.name + ' ' + this.localize.term('user_noLockouts')}`,
				)}
				${this.#renderInfoItem(
					'user_lastPasswordChangeDate',
					this._user.lastLoginDate || `${this._user.name + ' ' + this.localize.term('user_noPasswordChange')}`,
				)}
				${this.#renderInfoItem('user_createDate', this.localize.date(this._user.createDate!))}
				${this.#renderInfoItem('user_updateDate', this.localize.date(this._user.updateDate!))}
				${this.#renderInfoItem('general_id', this._user.id)}
			</div>
		</uui-box>`;
	}

	#renderInfoItem(labelkey: string, value?: string | number) {
		return html`
			<div>
				<b><umb-localize key=${labelkey}></umb-localize></b>
				<span>${value}</span>
			</div>
		`;
	}

	#renderActionButtons() {
		if (!this._user) return nothing;

		//TODO: Find out if the current user is an admin. If not, show no buttons.
		// if (this._currentUserStore?.isAdmin === false) return nothing;

		const buttons: TemplateResult[] = [];

		if (this._user.id !== this._currentUser?.id) {
			if (this._user.state === UserStateModel.DISABLED) {
				buttons.push(html`
					<uui-button
						@click=${this.#onUserStatusChange}
						look="secondary"
						color="positive"
						label=${this.localize.term('actions_enable')}></uui-button>
				`);
			}

			if (this._user.state === UserStateModel.ACTIVE || this._user.state === UserStateModel.INACTIVE) {
				buttons.push(html`
					<uui-button
						@click=${this.#onUserStatusChange}
						look="secondary"
						color="warning"
						label=${this.localize.term('actions_disable')}></uui-button>
				`);
			}
		}

		if (this._currentUser?.id !== this._user?.id) {
			const button = html`
				<uui-button
					@click=${this.#onUserDelete}
					look="secondary"
					color="danger"
					label=${this.localize.term('user_deleteUser')}></uui-button>
			`;

			buttons.push(button);
		}

		buttons.push(
			html`<uui-button
				@click=${this.#onPasswordChange}
				look="secondary"
				label=${this.localize.term('general_changePassword')}></uui-button>`,
		);

		return buttons;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#header {
				width: 100%;
				display: grid;
				grid-template-columns: var(--uui-size-layout-1) 1fr;
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
		`,
	];
}

export default UmbUserWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-editor': UmbUserWorkspaceEditorElement;
	}
}
