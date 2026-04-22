import { UmbCurrentUserRepository } from '../../repository/index.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserModel } from '../../types.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-current-user-edit-profile-settings')
export class UmbCurrentUserEditProfileSettingsElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbCurrentUserModel;

	@state()
	private _languageIsoCode = '';

	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this._currentUser = currentUser;
					if (currentUser) {
						this._languageIsoCode = currentUser.languageIsoCode;
					}
				},
				'umbCurrentUserObserver',
			);
		});
	}

	#onLanguageChange(event: UmbChangeEvent & { target: UmbUiCultureInputElement }) {
		if (typeof event.target?.value === 'string') {
			this._languageIsoCode = event.target.value;
		}
	}

	async save(): Promise<boolean> {
		if (this._languageIsoCode === this._currentUser?.languageIsoCode) return true;
		const { error } = await this.#currentUserRepository.updateProfile(this._languageIsoCode);
		return !error;
	}

	override render() {
		return html`<uui-box .headline=${this.localize.term('user_profile')}>${this.#renderLanguage()}</uui-box>`;
	}

	#renderLanguage() {
		return html`
			<umb-property-layout
				label=${this.localize.term('user_language')}
				description=${this.localize.term('user_languageHelp')}>
				<umb-ui-culture-input
					slot="editor"
					name="language"
					label=${this.localize.term('user_language')}
					value=${this._languageIsoCode}
					@change=${this.#onLanguageChange}>
				</umb-ui-culture-input>
			</umb-property-layout>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbCurrentUserEditProfileSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-edit-profile-settings': UmbCurrentUserEditProfileSettingsElement;
	}
}
