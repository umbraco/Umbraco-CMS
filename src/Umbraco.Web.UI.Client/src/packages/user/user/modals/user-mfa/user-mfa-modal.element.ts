import { UmbUserRepository } from '../../repository/index.js';
import type { UmbUserMfaProviderModel } from '../../types.js';
import type { UmbUserMfaModalConfiguration } from './user-mfa-modal.token.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbConfirmModal, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';

type UmbMfaLoginProviderOption = UmbUserMfaProviderModel & {
	displayName: string;
	existsOnServer: boolean;
};

@customElement('umb-user-mfa-modal')
export class UmbUserMfaModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbUserMfaModalConfiguration, never>;

	@state()
	_items: Array<UmbMfaLoginProviderOption> = [];

	#unique = '';
	#userRepository = new UmbUserRepository(this);

	override firstUpdated() {
		this.#unique = this.modalContext?.data.unique ?? '';
		this.#loadProviders();
	}

	async #loadProviders() {
		const serverLoginProviders$ = (await this.#userRepository.requestMfaProviders(this.#unique)).asObservable();
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
							disabled
							type="button"
							look="secondary"
							.label=${this.localize.term('actions_enable')}></uui-button>
					`,
				)}
			</uui-box>
		`;
	}

	/**
	 * This method is called when the user clicks the disable button on a provider.
	 * It will show a confirmation dialog and then disable the provider if the user confirms.
	 * NB! The user must have administrative rights before doing so.
	 * @param item
	 */
	async #onProviderDisable(item: UmbMfaLoginProviderOption) {
		await umbConfirmModal(this, {
			headline: this.localize.term('actions_disable'),
			content: this.localize.term('user_2faDisableForUser', item.displayName),
			confirmLabel: this.localize.term('actions_disable'),
			color: 'danger',
		});

		await this.#userRepository.disableMfaProvider(this.#unique, item.providerName, item.displayName);
		this.#loadProviders();
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbUserMfaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-mfa-modal': UmbUserMfaModalElement;
	}
}
