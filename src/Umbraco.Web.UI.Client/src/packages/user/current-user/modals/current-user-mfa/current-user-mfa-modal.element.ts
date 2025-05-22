import { UMB_CURRENT_USER_MFA_ENABLE_PROVIDER_MODAL } from '../current-user-mfa-enable/current-user-mfa-enable-modal.token.js';
import { UmbCurrentUserRepository } from '../../repository/index.js';
import { UMB_CURRENT_USER_MFA_DISABLE_PROVIDER_MODAL } from '../current-user-mfa-disable/current-user-mfa-disable-modal.token.js';
import type { UmbCurrentUserMfaProviderModel } from '../../types.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbOpenModal, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';

type UmbMfaLoginProviderOption = UmbCurrentUserMfaProviderModel & {
	displayName: string;
	existsOnServer: boolean;
};

@customElement('umb-current-user-mfa-modal')
export class UmbCurrentUserMfaModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	_items: Array<UmbMfaLoginProviderOption> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();
		this.#loadProviders();
	}

	async #loadProviders() {
		const serverLoginProviders$ = (await this.#currentUserRepository.requestMfaLoginProviders()).asObservable();
		const manifestLoginProviders$ = umbExtensionsRegistry.byType('mfaLoginProvider');

		// Merge the server and manifest providers to get the final list of providers
		const mfaLoginProviders$ = mergeObservables(
			[serverLoginProviders$, manifestLoginProviders$],
			([serverLoginProviders, manifestLoginProviders]) => {
				return manifestLoginProviders.map((manifestLoginProvider) => {
					const serverLoginProvider = serverLoginProviders.find(
						(serverLoginProvider) => serverLoginProvider.providerName === manifestLoginProvider.forProviderName,
					);
					return {
						existsOnServer: !!serverLoginProvider,
						isEnabledOnUser: serverLoginProvider?.isEnabledOnUser ?? false,
						providerName: serverLoginProvider?.providerName ?? manifestLoginProvider.forProviderName,
						displayName:
							manifestLoginProvider.meta?.label ?? serverLoginProvider?.providerName ?? manifestLoginProvider.name,
					} satisfies UmbMfaLoginProviderOption;
				});
			},
		);

		this.observe(
			mfaLoginProviders$,
			(providers) => {
				this._items = providers;
			},
			'_mfaLoginProviders',
		);
	}

	#close() {
		this.modalContext?.submit();
	}

	override render() {
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
					<uui-button @click=${this.#close} look="secondary" .label=${this.localize.term('general_close')}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	/**
	 * Render a provider with a toggle to enable/disable it
	 * @param item
	 */
	#renderProvider(item: UmbMfaLoginProviderOption) {
		return html`
			<uui-box headline=${item.displayName}>
				${when(
					item.existsOnServer,
					() => nothing,
					() =>
						html`<div style="position:relative" slot="header-actions">
							<uui-badge
								style="cursor:default"
								title="Warning: This provider is not configured on the server"
								color="danger"
								look="primary">
								!
							</uui-badge>
						</div>`,
				)}
				${when(
					item.isEnabledOnUser,
					() => html`
						<p style="margin-top:0">
							<umb-localize key="user_2faProviderIsEnabled">This two-factor provider is enabled</umb-localize>
						</p>
						<uui-button
							type="button"
							look="secondary"
							color="danger"
							.label=${this.localize.term('actions_disable')}
							@click=${() => this.#onProviderDisable(item)}></uui-button>
					`,
					() => html`
						<uui-button
							type="button"
							look="secondary"
							?disabled=${!item.existsOnServer}
							.label=${this.localize.term('actions_enable')}
							@click=${() => this.#onProviderEnable(item)}></uui-button>
					`,
				)}
			</uui-box>
		`;
	}

	/**
	 * Open the provider modal.
	 * This will show the QR code and/or other means of validation for the given provider and return the activation code.
	 * The activation code is then used to either enable the provider.
	 * @param item
	 */
	async #onProviderEnable(item: UmbMfaLoginProviderOption) {
		await umbOpenModal(this, UMB_CURRENT_USER_MFA_ENABLE_PROVIDER_MODAL, {
			data: { providerName: item.providerName, displayName: item.displayName },
		}).catch(() => undefined);
	}

	/**
	 * Open the provider modal.
	 * This will show the QR code and/or other means of validation for the given provider and return the activation code.
	 * The activation code is then used to disable the provider.
	 * @param item
	 */
	async #onProviderDisable(item: UmbMfaLoginProviderOption) {
		await umbOpenModal(this, UMB_CURRENT_USER_MFA_DISABLE_PROVIDER_MODAL, {
			data: { providerName: item.providerName, displayName: item.displayName },
		}).catch(() => undefined);
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbCurrentUserMfaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-modal': UmbCurrentUserMfaModalElement;
	}
}
