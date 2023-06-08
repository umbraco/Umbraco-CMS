import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../../current-user.store.js';
import type { UmbLoggedInUser } from '../../types.js';
import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';
import { UMB_APP } from '@umbraco-cms/backoffice/context';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, CSSResultGroup, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-current-user-modal')
export class UmbCurrentUserModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	private _currentUser?: UmbLoggedInUser;

	private _currentUserStore?: UmbCurrentUserStore;

	#auth?: typeof UMB_AUTH.TYPE;

	#appContext?: typeof UMB_APP.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (instance) => {
			this._currentUserStore = instance;
			this._observeCurrentUser();
		});

		this.consumeContext(UMB_AUTH, (instance) => {
			this.#auth = instance;
		});

		this.consumeContext(UMB_APP, (instance) => {
			this.#appContext = instance;
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
		this.modalContext?.submit();
	}

	private async _logout() {
		if (!this.#auth) return;
		this.#auth.performWithFreshTokens;
		await this.#auth.signOut();
		let newUrl = this.#appContext ? `${this.#appContext.getBackofficePath()}/login` : '/';
		newUrl = newUrl.replace(/\/\//g, '/');
		location.href = newUrl;
	}

	render() {
		return html`
			<umb-body-layout headline="${this._currentUser?.name || ''}">
				<div id="main">
					<umb-extension-slot id="userProfileApps" type="userProfileApp"></umb-extension-slot>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
					<uui-button @click=${this._logout} look="primary" color="danger">Logout</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#main {
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
}

export default UmbCurrentUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-modal': UmbCurrentUserModalElement;
	}
}
