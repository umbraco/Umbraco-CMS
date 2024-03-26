import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserMfaDisableProviderModalConfig } from './current-user-mfa-disable-provider-modal.token.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import '../../components/mfa-provider-default.element.js';

@customElement('umb-current-user-mfa-disable-provider-modal')
export class UmbCurrentUserMfaDisableProviderModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaDisableProviderModalConfig,
	never
> {
	#currentUserRepository = new UmbCurrentUserRepository(this);

	render() {
		if (!this.data) {
			return html`<uui-loader-bar></uui-loader-bar>`;
		}

		return html`
			<umb-mfa-provider-default
				.providerName=${this.data.providerName}
				.callback=${this.#disableProvider}
				.close=${() => this.modalContext?.submit()}
				success-message="This two-factor provider is now disabled"
				success-message-key="user_2faProviderIsDisabledMsg">
				<p slot="description">
					<umb-localize key="user_2faDisableText">
						If you wish to disable this two-factor provider, then you must enter the code shown on your authentication
						device:
					</umb-localize>
				</p>
			</umb-mfa-provider-default>
		`;
	}

	#disableProvider = (providerName: string, code: string): Promise<boolean> => {
		return this.#currentUserRepository.disableMfaProvider(providerName, code);
	};
}

export default UmbCurrentUserMfaDisableProviderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-disable-provider-modal': UmbCurrentUserMfaDisableProviderModalElement;
	}
}
