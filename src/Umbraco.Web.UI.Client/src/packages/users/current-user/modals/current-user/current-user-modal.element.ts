import { UMB_APP } from '@umbraco-cms/backoffice/app';
import { UMB_AUTH, type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, CSSResultGroup, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-current-user-modal')
export class UmbCurrentUserModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

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
					<uui-button @click=${this._close} look="secondary" .label=${this.localize.term('general_close')}>
						${this.localize.term('general_close')}
					</uui-button>
					<uui-button
						@click=${this._logout}
						look="primary"
						color="danger"
						.label=${this.localize.term('general_logout')}>
						${this.localize.term('general_logout')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
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
