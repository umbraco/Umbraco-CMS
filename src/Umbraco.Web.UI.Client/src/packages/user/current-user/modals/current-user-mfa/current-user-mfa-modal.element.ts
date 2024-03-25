import { UmbCurrentUserRepository } from '../../repository/index.js';
import { customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-current-user-mfa-modal')
export class UmbCurrentUserMfaModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	_items: Array<UserTwoFactorProviderModel> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();
		this.#loadProviders();
	}

	async #loadProviders() {
		const { data: providers } = await this.#currentUserRepository.requestMfaLoginProviders();

		if (!providers) return;

		this._items = providers;
	}

	#close() {
		this.modalContext?.submit();
	}

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('member_2fa')}">
				<div id="main">
					${when(
						this._items.length > 0,
						() => html`
							${repeat(
								this._items,
								(item) => item.providerName,
								(item) => this.#renderProvider(item),
							)}
						`,
					)}
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" .label=${this.localize.term('general_close')}>
						${this.localize.term('general_close')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	/**
	 * Render a provider with a toggle to enable/disable it
	 */
	#renderProvider(item: UserTwoFactorProviderModel) {
		return html`
			<div>
				<uui-toggle
					label=${item.providerName}
					?checked=${item.isEnabledOnUser}
					@change=${() => this.#onProviderToggleChange(item)}></uui-toggle>
			</div>
		`;
	}

	#onProviderToggleChange = (item: UserTwoFactorProviderModel) => {
		// If already enabled, disable it
		if (item.isEnabledOnUser) {
			// Disable provider
			return;
		}

		// Enable provider
	};
}

export default UmbCurrentUserMfaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-modal': UmbCurrentUserMfaModalElement;
	}
}
