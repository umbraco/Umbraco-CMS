import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import {
	UmbCurrentUserHistoryStore,
	UmbCurrentUserHistoryItem,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from '../../current-user-history.store';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from '../../current-user.store';
import { UMB_CHANGE_PASSWORD_MODAL_TOKEN } from '../change-password';
import { UmbModalHandler, UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

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
			#recent-history {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			#recent-history-items {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.history-item {
				display: grid;
				grid-template-columns: 32px 1fr;
				grid-template-rows: 1fr;
				color: var(--uui-color-interactive);
				text-decoration: none;
			}
			.history-item uui-icon {
				margin-top: var(--uui-size-space-1);
			}
			.history-item:hover {
				color: var(--uui-color-interactive-emphasis);
			}
			.history-item > div {
				color: inherit;
				text-decoration: none;
				display: flex;
				flex-direction: column;
				line-height: 1.4em;
			}
			.history-item > div > span {
				font-size: var(--uui-size-4);
				opacity: 0.5;
				text-overflow: ellipsis;
				overflow: hidden;
				white-space: nowrap;
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@state()
	private _currentUser?: UserDetails;

	@state()
	private _history: Array<UmbCurrentUserHistoryItem> = [];

	private _modalContext?: UmbModalContext;
	private _currentUserStore?: UmbCurrentUserStore;
	private _currentUserHistoryStore?: UmbCurrentUserHistoryStore;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (instance) => {
			this._currentUserStore = instance;
			this._observeCurrentUser();
		});

		this.consumeContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN, (instance) => {
			this._currentUserHistoryStore = instance;
			this._observeHistory();
		});

		this._observeCurrentUser();
	}

	private async _observeCurrentUser() {
		if (!this._currentUserStore) return;

		this.observe(this._currentUserStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}
	private async _observeHistory() {
		if (this._currentUserHistoryStore) {
			this.observe(this._currentUserHistoryStore.latestHistory, (history) => {
				this._history = history;
			});
		}
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

	private _renderHistoryItem(item: UmbCurrentUserHistoryItem) {
		return html`
			<a href=${item.path} class="history-item">
				<uui-icon name="umb:link"></uui-icon>
				<div>
					<b>${Array.isArray(item.label) ? item.label[0] : item.label}</b>
					<span>
						${Array.isArray(item.label)
							? item.label.map((label, index) => {
									if (index === 0) return;
									return html`
										<span>${label}</span>
										${index !== item.label.length - 1 ? html`<span>${'>'}</span>` : nothing}
									`;
							  })
							: nothing}
					</span>
				</div>
			</a>
		`;
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
					<uui-box>
						<b slot="headline">External login providers</b>
						<umb-extension-slot id="externalLoginProviders" type="externalLoginProvider"></umb-extension-slot>
					</uui-box>
					<div>
						<umb-extension-slot id="userDashboards" type="userDashboard"></umb-extension-slot>
					</div>
					<uui-box>
						<b slot="headline">Recent History</b>
						<div id="recent-history-items">
							${this._history.reverse().map((item) => html` ${this._renderHistoryItem(item)} `)}
						</div>
					</uui-box>
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
