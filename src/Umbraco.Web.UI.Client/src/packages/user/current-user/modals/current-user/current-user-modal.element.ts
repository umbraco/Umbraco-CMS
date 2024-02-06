import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.js';
import type { UmbCurrentUser } from '../../types.js';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup} from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-current-user-modal')
export class UmbCurrentUserModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	private _currentUser?: UmbCurrentUser;

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#appContext?: typeof UMB_APP_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			this._observeCurrentUser();
		});

		this.consumeContext(UMB_AUTH_CONTEXT, (instance) => {
			this.#authContext = instance;
		});

		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#appContext = instance;
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

	private _close() {
		this.modalContext?.submit();
	}

	private async _logout() {
		if (!this.#authContext) return;
		this.#authContext.signOut();
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
