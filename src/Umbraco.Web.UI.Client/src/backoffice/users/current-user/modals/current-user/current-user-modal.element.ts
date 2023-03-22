import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbModalHandler, UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_CHANGE_PASSWORD_MODAL_TOKEN } from '../change-password';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../../current-user.store';

@customElement('umb-current-user-modal')
export class UmbCurrentUserModalElement extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}
			:host,
			umb-workspace-layout {
				width: 100%;
				height: 100%;
			}
			#main {
				padding: var(--uui-size-space-5);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			#umbraco-id-buttons {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			#userProfileApps {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@state()
	private _currentUser?: UserDetails;

	private _modalContext?: UmbModalContext;
	private _currentUserStore?: UmbCurrentUserStore;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (instance) => {
			this._currentUserStore = instance;
			this._observeCurrentUser();
		});

		this._observeCurrentUser();
	}

	private async _observeCurrentUser() {
		if (!this._currentUserStore) return;

		this.observe(this._currentUserStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _close() {
		this.modalHandler?.submit();
	}

	private _edit() {
		if (!this._currentUser) return;

		history.pushState(null, '', '/section/users/view/users/user/' + this._currentUser.key); //TODO Change to a tag with href and make dynamic
		this._close();
	}

	private _changePassword() {
		if (!this._modalContext) return;

		this._modalContext.open(UMB_CHANGE_PASSWORD_MODAL_TOKEN, {
			requireOldPassword: this._currentUserStore?.isAdmin || false,
		});
	}

	private _logout() {
		this._currentUserStore?.logout();
	}

	render() {
		return html`
			<umb-workspace-layout headline="${this._currentUser?.name || ''}">
				<div id="main">
					<uui-box>
						<b slot="headline">Your profile</b>
						<uui-button look="primary" @click=${this._edit}>Edit</uui-button>
						<uui-button look="primary" @click=${this._changePassword}>Change password</uui-button>
					</uui-box>

					<umb-extension-slot id="userProfileApps" type="userProfileApp"></umb-extension-slot>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
					<uui-button @click=${this._logout} look="primary" color="danger">Logout</uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbCurrentUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-modal': UmbCurrentUserModalElement;
	}
}
