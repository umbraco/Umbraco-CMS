import type { UmbMfaProviderConfigurationElementProps } from '../../types.js';
import type {
	UmbCurrentUserMfaProviderModalConfig,
	UmbCurrentUserMfaProviderModalValue,
} from './current-user-mfa-provider-modal.token.js';
import type { ManifestMfaLoginProvider } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import './mfa-provider-default.element.js';

@customElement('umb-current-user-mfa-provider-modal')
export class UmbCurrentUserMfaProviderModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaProviderModalConfig,
	UmbCurrentUserMfaProviderModalValue
> {
	#submit = (value: UmbCurrentUserMfaProviderModalValue) => {
		this.value = value;
		this._submitModal();
	};

	#close = () => {
		this._rejectModal();
	};

	get #extensionSlotProps(): UmbMfaProviderConfigurationElementProps {
		return {
			providerName: this.data!.providerName,
			isEnabled: this.data!.isEnabled,
			onSubmit: this.#submit,
			onClose: this.#close,
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
