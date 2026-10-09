import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UmbCurrentUserRepository } from '../repository/current-user.repository.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-current-user-language-user-profile-app')
export class UmbCurrentUserLanguageUserProfileAppElement extends UmbLitElement {
	#repository = new UmbCurrentUserRepository(this);

	@state()
	private _languageIsoCode?: string;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.languageIsoCode,
				(languageIsoCode) => (this._languageIsoCode = languageIsoCode),
				'umbCurrentUserLanguageObserver',
			);
		});
	}

	async #onLanguageChange(event: UmbChangeEvent & { target: UmbUiCultureInputElement }) {
		const languageIsoCode = event.target.value;
		if (!languageIsoCode || languageIsoCode === this._languageIsoCode?.toLowerCase()) return;

		const { error } = await this.#repository.updateProfile(languageIsoCode);
		if (error) {
			// Revert the input to the persisted language, as the repository has already notified about the failure.
			event.target.value = this._languageIsoCode;
		}
	}

	override render() {
		return html`
			<uui-box .headline=${this.localize.term('user_language')}>
				<umb-ui-culture-input
					name="language"
					label=${this.localize.term('user_language')}
					value=${this._languageIsoCode ?? ''}
					@change=${this.#onLanguageChange}>
				</umb-ui-culture-input>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			umb-ui-culture-input {
				display: block;
			}
		`,
	];
}

export default UmbCurrentUserLanguageUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-language-user-profile-app': UmbCurrentUserLanguageUserProfileAppElement;
	}
}
