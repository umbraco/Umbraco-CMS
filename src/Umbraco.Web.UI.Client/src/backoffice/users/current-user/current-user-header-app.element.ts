import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './current-user.store';
import { UMB_CURRENT_USER_MODAL_TOKEN } from './modals/current-user';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderApp extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 14px;
				--uui-button-background-color: transparent;
			}
		`,
	];

	@state()
	private _currentUser?: UserDetails;

	private _currentUserStore?: UmbCurrentUserStore;
	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (instance) => {
			this._currentUserStore = instance;
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this._currentUserStore) return;

		this.observe(this._currentUserStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _handleUserClick() {
		this._modalContext?.open(UMB_CURRENT_USER_MODAL_TOKEN);
	}

	render() {
		return html`
			<uui-button @click=${this._handleUserClick} look="primary" label="${this._currentUser?.name || ''}" compact>
				<uui-avatar name="${this._currentUser?.name || ''}"></uui-avatar>
			</uui-button>
		`;
	}
}

export default UmbCurrentUserHeaderApp;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-header-app': UmbCurrentUserHeaderApp;
	}
}
