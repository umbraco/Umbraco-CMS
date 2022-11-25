import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbModalHandler, UmbModalService } from '@umbraco-cms/services';
import type { ManifestExternalLoginProvider, UserDetails } from '@umbraco-cms/models';
import { UmbUserStore } from 'src/core/stores/user/user.store';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import '../../../../backoffice/external-login-providers/external-login-provider-extension.element';

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
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@state()
	private _currentUser?: UserDetails;

	@state()
	private _externalLoginProviders: Array<ManifestExternalLoginProvider> = [];

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
	}

	private async _observeCurrentUser() {
		if (!this._userStore) return;

		this.observe<UserDetails>(this._userStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _observeExternalLoginProviders() {
		this.observe<ManifestExternalLoginProvider[]>(
			umbExtensionsRegistry.extensionsOfType('externalLoginProvider'),
			(loginProvider) => {
				this._externalLoginProviders = loginProvider;
				console.log('loginProvider', loginProvider);
			}
		);
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
						<uui-button look="primary">Edit your Umbraco ID profile</uui-button>
						<uui-button look="primary">Change your Umbraco ID password</uui-button>
					</uui-box>

					<div>
						${this._externalLoginProviders.map(
							(provider) =>
								html`<umb-external-login-provider-extension
									.externalLoginProvider=${provider}></umb-external-login-provider-extension>`
						)}
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
