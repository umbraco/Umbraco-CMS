import { UmbCurrentUserRepository } from '../repository/index.js';
import { html, customElement, state, when, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	type UmbExtensionElementInitializer,
	UmbExtensionsElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { type ManifestMfaLoginProvider, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-mfa-providers-user-profile-app')
export class UmbMfaProvidersUserProfileAppElement extends UmbLitElement {
	@state()
	_items: Array<UmbExtensionElementInitializer<ManifestMfaLoginProvider>> = [];

	#currentUserRepository = new UmbCurrentUserRepository(this);
	#extensionsInitializer?: UmbExtensionsElementInitializer<
		ManifestMfaLoginProvider,
		'mfaLoginProvider',
		ManifestMfaLoginProvider
	>;

	constructor() {
		super();

		this.#extensionsInitializer = new UmbExtensionsElementInitializer<
			ManifestMfaLoginProvider,
			'mfaLoginProvider',
			ManifestMfaLoginProvider
		>(
			this,
			umbExtensionsRegistry,
			'mfaLoginProvider',
			undefined,
			(permitted) => (this._items = permitted),
			'_mfaLoginProviders',
			'umb-mfa-login-provider-default',
		);

		this.#loadProviders();
	}

	async #loadProviders() {
		const { data: providers } = await this.#currentUserRepository.requestMfaLoginProviders();

		if (!providers) return;

		for (const provider of providers) {
			// Check if provider is initialized as extension
			const extension = this._items.find((item) => item.manifest?.forProviderNames.includes(provider.providerName));
			if (extension) {
				extension.properties = { provider };
			} else {
				// Register provider as extension
				const manifest: ManifestMfaLoginProvider = {
					type: 'mfaLoginProvider',
					alias: provider.providerName,
					name: provider.providerName,
					forProviderNames: [provider.providerName],
					meta: {
						label: provider.providerName,
					},
				};
				umbExtensionsRegistry.register(manifest);
			}
		}
	}

	render() {
		return when(
			this._items.length > 0,
			() => html`
				<uui-box headline=${this.localize.term('member_2fa')}>
					${repeat(
						this._items,
						(item) => item.alias,
						(item) => item.component,
					)}
				</uui-box>
			`,
		);
	}

	static styles = [UmbTextStyles];
}

export default UmbMfaProvidersUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-mfa-providers-user-profile-app': UmbMfaProvidersUserProfileAppElement;
	}
}
