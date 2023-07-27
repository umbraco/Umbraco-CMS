import { UMB_LOCALIZATION_CONTEXT } from '@umbraco-cms/backoffice/localization-api';
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

	@state()
	protected _labelYourProfile = 'Your profile';

	@state()
	protected _labelEditProfile = 'Edit';

	@state()
	protected _labelChangePassword = 'Change password';

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

		this.consumeContext(UMB_LOCALIZATION_CONTEXT, (instance) => {
			instance.localizeMany(['user_yourProfile', 'general_edit', 'general_changePassword']).subscribe((value) => {
				this._labelYourProfile = value[0];
				this._labelEditProfile = value[1];
				this._labelChangePassword = value[2];
			});
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
			<uui-box .headline=${this._labelYourProfile}>
				<uui-button look="primary" label=${this._labelEditProfile} @click=${this._edit}>
					${this._labelEditProfile}
				</uui-button>
				<uui-button look="primary" label=${this._labelChangePassword} @click=${this._changePassword}>
					${this._labelChangePassword}
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
