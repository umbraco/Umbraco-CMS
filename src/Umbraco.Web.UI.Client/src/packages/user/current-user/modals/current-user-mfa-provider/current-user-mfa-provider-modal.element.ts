import type { UmbMfaProviderConfigurationElementProps } from '../../types.js';
import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserMfaProviderModalConfig } from './current-user-mfa-provider-modal.token.js';
import type { ManifestMfaLoginProvider } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import '../../components/mfa-provider-default.element.js';

@customElement('umb-current-user-mfa-provider-modal')
export class UmbCurrentUserMfaProviderModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaProviderModalConfig,
	never
> {
	#currentUserRepository = new UmbCurrentUserRepository(this);

	#close = () => {
		this._rejectModal();
	};

	get #extensionSlotProps(): UmbMfaProviderConfigurationElementProps {
		return {
			providerName: this.data!.providerName,
			enableProvider: (providerName, code, secret) =>
				this.#currentUserRepository.enableMfaProvider(providerName, code, secret),
			close: this.#close,
		};
	}

	render() {
		if (!this.data) {
			return html`<uui-loader-bar></uui-loader-bar>`;
		}

		return html`
			<umb-extension-slot
				type="mfaLoginProvider"
				default-element="umb-mfa-provider-default"
				.filter=${this.#extensionSlotFilter}
				.props=${this.#extensionSlotProps}></umb-extension-slot>
		`;
	}

	#extensionSlotFilter = (manifest: ManifestMfaLoginProvider): boolean => {
		if (!this.data) return false;
		const providerName = this.data.providerName.toLowerCase();
		return !manifest.forProviderNames || manifest.forProviderNames.includes(providerName);
	};
}

export default UmbCurrentUserMfaProviderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-provider-modal': UmbCurrentUserMfaProviderModalElement;
	}
}
