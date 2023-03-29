import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../current-user.store';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbModalContext, UMB_CHANGE_PASSWORD_MODAL, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

@customElement('umb-user-profile-app-profile')
export class UmbUserProfileAppProfileElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

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

	private _edit() {
		if (!this._currentUser) return;

		history.pushState(null, '', '/section/users/view/users/user/' + this._currentUser.key); //TODO Change to a tag with href and make dynamic
		//TODO Implement modal routing for the current-user-modal, so that the modal closes when navigating to the edit profile page
	}
	private _changePassword() {
		if (!this._modalContext) return;

		this._modalContext.open(UMB_CHANGE_PASSWORD_MODAL, {
			requireOldPassword: this._currentUserStore?.isAdmin || false,
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
}

export default UmbUserProfileAppProfileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-profile': UmbUserProfileAppProfileElement;
	}
}
