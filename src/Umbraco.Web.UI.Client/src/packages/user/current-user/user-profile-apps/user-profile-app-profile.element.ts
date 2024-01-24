import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbModalManagerContext,
	UMB_CHANGE_PASSWORD_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
} from '@umbraco-cms/backoffice/modal';
import { UMB_CURRENT_USER_CONTEXT, type UmbCurrentUser } from '@umbraco-cms/backoffice/current-user';

@customElement('umb-user-profile-app-profile')
export class UmbUserProfileAppProfileElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbCurrentUser;

	#modalManagerContext?: UmbModalManagerContext;
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this.#currentUserContext) return;

		this.observe(
			this.#currentUserContext.currentUser,
			(currentUser) => {
				this._currentUser = currentUser;
			},
			'umbCurrentUserObserver',
		);
	}

	private _edit() {
		if (!this._currentUser) return;

		history.pushState(null, '', 'section/user-management/view/users/user/' + this._currentUser.id); //TODO Change to a tag with href and make dynamic
		//TODO Implement modal routing for the current-user-modal, so that the modal closes when navigating to the edit profile page
	}
	private _changePassword() {
		if (!this.#modalManagerContext) return;

		this.#modalManagerContext.open(UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				userId: this._currentUser?.id ?? '',
			},
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

	static styles = [UmbTextStyles, css``];
}

export default UmbUserProfileAppProfileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-profile': UmbUserProfileAppProfileElement;
	}
}
