import {
	UMB_CURRENT_USER_MFA_PROVIDER_MODAL,
	type UmbCurrentUserMfaProviderModalValue,
} from '../current-user-mfa-provider/current-user-mfa-provider-modal.token.js';
import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserMfaProviderModel } from '../../types.js';
import { customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT, type UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-current-user-mfa-modal')
export class UmbCurrentUserMfaModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	_items: Array<UmbCurrentUserMfaProviderModel> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();
		this.#loadProviders();
	}

	async #loadProviders() {
		const providers = await this.#currentUserRepository.requestMfaLoginProviders();
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
	#renderProvider(item: UmbCurrentUserMfaProviderModel) {
		return html`
			<div>
				<uui-toggle
					label=${item.providerName}
					?checked=${item.isEnabledOnUser}
					@change=${(e: Event) => this.#onProviderToggleChange(e, item)}></uui-toggle>
			</div>
		`;
	}

	async #onProviderToggleChange(event: Event, item: UmbCurrentUserMfaProviderModel) {
		// Prevent the toggle from changing
		event.preventDefault();
		event.stopPropagation();

		const { code, secret } = await this.#openProviderModal(item);

		// If no code, do nothing
		if (!code) {
			return;
		}

		// If already enabled, disable it
		if (item.isEnabledOnUser) {
			// Disable provider
			return this.#currentUserRepository.disableMfaProvider(item.providerName, code);
		}

		// Enable provider
		// If no secret, do nothing
		if (!secret) {
			return;
		}

		return this.#currentUserRepository.enableMfaProvider(item.providerName, code, secret);
	}

	/**
	 * Open the provider modal.
	 * This will show the QR code and/or other means of validation for the given provider and return the activation code.
	 * The activation code is then used to either enable or disable the provider.
	 */
	async #openProviderModal(item: UmbCurrentUserMfaProviderModel): Promise<UmbCurrentUserMfaProviderModalValue> {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		return modalManager
			.open(this, UMB_CURRENT_USER_MFA_PROVIDER_MODAL, {
				data: { providerName: item.providerName, isEnabled: item.isEnabledOnUser },
			})
			.onSubmit()
			.catch(() => ({}));
	}
}

export default UmbCurrentUserMfaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-modal': UmbCurrentUserMfaModalElement;
	}
}
