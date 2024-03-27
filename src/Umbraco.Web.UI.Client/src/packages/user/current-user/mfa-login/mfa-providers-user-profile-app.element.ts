import { UmbCurrentUserRepository } from '../repository/index.js';
import { UMB_CURRENT_USER_MFA_MODAL } from '../modals/current-user-mfa/current-user-mfa-modal.token.js';
import { html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-mfa-providers-user-profile-app')
export class UmbMfaProvidersUserProfileAppElement extends UmbLitElement {
	#currentUserRepository = new UmbCurrentUserRepository(this);

	@state()
	_hasProviders = false;

	constructor() {
		super();
		this.#init();
	}

	async #init() {
		this._hasProviders = (await firstValueFrom(umbExtensionsRegistry.byType('mfaLoginProvider'))).length > 0;
	}

	render() {
		if (!this._hasProviders) {
			return nothing;
		}

		return html`
			<uui-box .headline=${this.localize.term('member_2fa')}>
				<uui-button type="button" look="primary" @click=${this.#onClick}>
					<umb-localize key="user_configureTwoFactor">Configure Two Factor</umb-localize>
				</uui-button>
			</uui-box>
		`;
	}

	async #onClick() {
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		await modalManagerContext.open(this, UMB_CURRENT_USER_MFA_MODAL).onSubmit();
	}

	static styles = [UmbTextStyles];
}

export default UmbMfaProvidersUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-mfa-providers-user-profile-app': UmbMfaProvidersUserProfileAppElement;
	}
}
