import { UMB_LOCALIZATION_CONTEXT } from '@umbraco-cms/backoffice/localization-api';
import { UMB_AUTH, type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
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
	protected labelClose = 'Close';

	@state()
	protected labelLogout = 'Log out';

	@state()
	private _currentUser?: UmbLoggedInUser;

	#auth?: typeof UMB_AUTH.TYPE;

	#appContext?: typeof UMB_APP.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_AUTH, (instance) => {
			this.#auth = instance;
			this._observeCurrentUser();
		});

		this.consumeContext(UMB_APP, (instance) => {
			this.#appContext = instance;
		});

		this.consumeContext(UMB_LOCALIZATION_CONTEXT, (instance) => {
			instance.localizeMany(['general_close', 'general_logout']).subscribe((values) => {
				this.labelClose = values[0];
				this.labelLogout = values[1];
			});
		});
	}

	private async _observeCurrentUser() {
		if (!this.#auth) return;

		this.observe(this.#auth.currentUser, (currentUser) => {
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
					<uui-button @click=${this._close} look="secondary" label=${this.labelClose}> ${this.labelClose} </uui-button>
					<uui-button @click=${this._logout} look="primary" color="danger" label=${this.labelLogout}>
						${this.labelLogout}
					</uui-button>
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
