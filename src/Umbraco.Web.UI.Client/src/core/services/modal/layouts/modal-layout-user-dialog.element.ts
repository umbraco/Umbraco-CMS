import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbHistoryItem, umbHistoryService } from '../../history';
import { UmbModalHandler, UmbModalService } from '@umbraco-cms/services';
import type { ManifestExternalLoginProvider, ManifestUserDashboard, UserDetails } from '@umbraco-cms/models';
import { UmbUserStore } from 'src/core/stores/user/user.store';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import '../../../../backoffice/external-login-providers/external-login-provider-extension.element';
import '../../../../backoffice/user-dashboards/user-dashboard-extension.element';

@customElement('umb-modal-layout-user-dialog')
export class UmbModalLayoutUserDialogElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			:host,
			umb-editor-entity-layout {
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
			umb-external-login-provider-extension:not(:last-child) {
				margin-bottom: var(--uui-size-space-3);
				display: block;
			}
			#recent-history {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			#recent-history-items {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			.history-item {
				color: inherit;
				text-decoration: none;
			}
			.history-item-name-separator {
				margin: 0 var(--uui-size-space-1);
			}
			.history-item-path,
			.history-item-name-separator {
				opacity: 0.4;
			}
			.history-item-name,
			.history-item-path {
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
	private _externalLoginProviders: Array<ManifestExternalLoginProvider> = [];

	@state()
	private _userDashboards: Array<ManifestUserDashboard> = [];

	@state()
	private _history: Array<UmbHistoryItem> = [];

	private _userStore?: UmbUserStore;
	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeAllContexts(['umbUserStore', 'umbModalService'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._modalService = instances['umbModalService'];
			this._observeCurrentUser();
		});

		this._observeExternalLoginProviders();
		this._observeHistory();
		this._observeUserDashboards();
	}

	private _observeExternalLoginProviders() {
		this.observe<ManifestExternalLoginProvider[]>(
			umbExtensionsRegistry.extensionsOfType('externalLoginProvider'),
			(loginProvider) => {
				this._externalLoginProviders = loginProvider;
			}
		);
	}

	private async _observeCurrentUser() {
		if (!this._userStore) return;

		this.observe<UserDetails>(this._userStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}
	private async _observeHistory() {
		this.observe<Array<UmbHistoryItem>>(umbHistoryService.history, (history) => {
			this._history = history;
		});
	}

	private _observeUserDashboards() {
		this.observe<ManifestUserDashboard[]>(umbExtensionsRegistry.extensionsOfType('userDashboard'), (userDashboard) => {
			this._userDashboards = userDashboard;
			console.log(this._userDashboards);
		});
	}

	private _close() {
		this.modalHandler?.close();
	}

	private _edit() {
		if (!this._currentUser) return;
		history.pushState(null, '', '/section/users/view/users/user/' + this._currentUser.key); //TODO Change to a tag with href and make dynamic
		this._close();
	}

	private _changePassword() {
		if (!this._modalService) return;
		this._modalService.changePassword();
	}

	private _renderHistoryItem(item: UmbHistoryItem) {
		if (typeof item.label === 'string') {
			return html`<a class="history-item" href="${item.path}">
				<div class="history-item-name">${item.label}</div>
			</a>`;
		}
		if (Array.isArray(item.label)) {
			console.log('hallo', item);

			return html`
				<a class="history-item" href=${item.path}>
					<div>
						${item.label.map((name, index) => {
							return html`<span class="history-item-name">${name}</span
								><span class="history-item-name-separator">${index < item.label.length - 1 ? '>' : nothing}</span>`;
						})}
					</div>
				</a>
			`;
		}

		return nothing;
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="${this._currentUser?.name || ''}">
				<div id="main">
					<uui-box>
						<b slot="headline">Your profile</b>
						<uui-button look="primary" @click=${this._edit}>Edit</uui-button>
						<uui-button look="primary" @click=${this._changePassword}>Change password</uui-button>
					</uui-box>
					<uui-box>
						<b slot="headline">External login providers</b>
						${this._externalLoginProviders.map(
							(provider) =>
								html`<umb-external-login-provider-extension
									.externalLoginProvider=${provider}></umb-external-login-provider-extension>`
						)}
					</uui-box>
					<div>
						${this._userDashboards.map(
							(provider) =>
								html`<umb-user-dashboard-extension .userDashboard=${provider}></umb-user-dashboard-extension>`
						)}
					</div>
					<div id="recent-history">
						<b>Recent History</b>
						<div id="recent-history-items">${this._history.map((item) => html`${this._renderHistoryItem(item)}`)}</div>
					</div>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
					<uui-button look="primary" color="danger">Logout</uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-user-dialog': UmbModalLayoutUserDialogElement;
	}
}
