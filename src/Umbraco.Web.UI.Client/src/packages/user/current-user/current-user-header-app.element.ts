import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, CSSResultGroup, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CURRENT_USER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_AUTH, type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderAppElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbLoggedInUser;

	private _auth?: typeof UMB_AUTH.TYPE;
	private _modalContext?: UmbModalManagerContext;

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

	private _handleUserClick() {
		this._modalContext?.open(UMB_CURRENT_USER_MODAL);
	}

	render() {
		return html`
			<uui-button @click=${this._handleUserClick} look="primary" label="${this._currentUser?.name || ''}" compact>
				<uui-avatar name="${this._currentUser?.name || ''}"></uui-avatar>
			</uui-button>
		`;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			uui-button {
				font-size: 14px;
				--uui-button-background-color: transparent;
			}
		`,
	];
}

export default UmbCurrentUserHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-header-app': UmbCurrentUserHeaderAppElement;
	}
}
