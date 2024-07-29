import type { UmbMfaProviderConfigurationElementProps } from '../../types.js';
import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserMfaEnableModalConfig } from './current-user-mfa-enable-modal.token.js';
import type { ManifestMfaLoginProvider } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import '../../components/mfa-provider-default.element.js';

@customElement('umb-current-user-mfa-enable-modal')
export class UmbCurrentUserMfaEnableModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaEnableModalConfig,
	never
> {
	#currentUserRepository = new UmbCurrentUserRepository(this);

	#close = () => {
		this._rejectModal();
	};

	get #extensionSlotProps(): UmbMfaProviderConfigurationElementProps {
		return {
			providerName: this.data!.providerName,
			displayName: this.data!.displayName,
			callback: (providerName, code, secret) =>
				this.#currentUserRepository.enableMfaProvider(providerName, code, secret),
			close: this.#close,
		};
	}

	override render() {
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
		return manifest.forProviderName.toLowerCase() === this.data.providerName.toLowerCase();
	};
}

export default UmbCurrentUserMfaEnableModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-enable-modal': UmbCurrentUserMfaEnableModalElement;
	}
}
