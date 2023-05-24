import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../current-user.store.js';
import type { UmbLoggedInUser } from '../types.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext, UMB_CHANGE_PASSWORD_MODAL, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

@customElement('umb-user-profile-app-profile')
export class UmbUserProfileAppProfileElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbLoggedInUser;

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

	private _edit() {
		if (!this._currentUser) return;

		history.pushState(null, '', 'section/users/view/users/user/' + this._currentUser.id); //TODO Change to a tag with href and make dynamic
		//TODO Implement modal routing for the current-user-modal, so that the modal closes when navigating to the edit profile page
	}
	private _changePassword() {
		if (!this._modalContext) return;

		// TODO: check if current user is admin
		this._modalContext.open(UMB_CHANGE_PASSWORD_MODAL, {
			requireOldPassword: false,
		});
	}

	render() {
		return html`
			<uui-box>
				<b slot="headline">Your profile</b>
				<uui-button look="primary" @click=${this._edit}>Edit</uui-button>
				<uui-button look="primary" @click=${this._changePassword}>Change password</uui-button>
			</uui-box>
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbUserProfileAppProfileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-profile': UmbUserProfileAppProfileElement;
	}
}
