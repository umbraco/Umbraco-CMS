import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbModalManagerContext,
	UMB_CHANGE_PASSWORD_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';
import { UMB_AUTH, type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';

@customElement('umb-user-profile-app-profile')
export class UmbUserProfileAppProfileElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbLoggedInUser;

	private _modalContext?: UmbModalManagerContext;
	private _auth?: typeof UMB_AUTH.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_AUTH, (instance) => {
			this._auth = instance;
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this._auth) return;

		this.observe(this._auth.currentUser, (currentUser) => {
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
			<uui-box .headline=${this.localize.term('user_yourProfile')}>
				<uui-button look="primary" label=${this.localize.term('general_edit')} @click=${this._edit}>
					${this.localize.term('general_edit')}
				</uui-button>
				<uui-button look="primary" label=${this.localize.term('general_changePassword')} @click=${this._changePassword}>
					${this.localize.term('general_changePassword')}
				</uui-button>
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
