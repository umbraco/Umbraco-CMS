import type { UmbCurrentUserMfaProviderModel } from '../../types.js';
import { customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type {
	UmbCurrentUserMfaProviderModalConfig,
	UmbCurrentUserMfaProviderModalValue,
} from './current-user-mfa-provider-modal.token.js';

@customElement('umb-current-user-mfa-provider-modal')
export class UmbCurrentUserMfaProviderModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaProviderModalConfig,
	UmbCurrentUserMfaProviderModalValue
> {
	#submit() {
		this._submitModal();
	}

	#close() {
		this._rejectModal();
	}

	render() {
		return html`
			<umb-body-layout headline="${this.data?.providerName ?? 'Configuration'}">
				<div id="main"></div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" .label=${this.localize.term('general_close')}>
						${this.localize.term('general_close')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export default UmbCurrentUserMfaProviderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-provider-modal': UmbCurrentUserMfaProviderModalElement;
	}
}
