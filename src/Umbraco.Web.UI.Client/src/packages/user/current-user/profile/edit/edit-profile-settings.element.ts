import type { UmbCurrentUserModel } from '../../types.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import { UmbCurrentUserRepository } from '../../repository/index.js';
import { html, customElement, state, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-current-user-workspace-profile-settings')
export class UmbCurrentUserWorkspaceProfileSettingsElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbCurrentUserModel;
	
	@state()
	private _languageIsoCode = '';

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			if (!this.#currentUserContext) return;
			this.#observeCurrentUser();
		});
	}

	#observeCurrentUser = () => {
		this.observe(
			this.#currentUserContext!.currentUser,
			(user) => {
				this._currentUser = user;
				if (user) {
					this._languageIsoCode = user.languageIsoCode;
				}
			},
			'umbCurrentUserObserver',
		);
	};

	#onLanguageChange(event: UmbChangeEvent) {
		const target = event.target as UmbUiCultureInputElement;
		if (typeof target?.value === 'string') {
			this._languageIsoCode = target.value;
		}
	}

	async save() {
		if (this._languageIsoCode === this._currentUser?.languageIsoCode) return;

		await this.#currentUserRepository.updateProfile(this._languageIsoCode);
	}

	override render() {
		return html`
			<uui-box>
				<div slot="headline"><umb-localize key="user_profile">Profile</umb-localize></div>
				${this.#renderUILanguageProperty()}
			</uui-box>
		`;
	}

	#renderUILanguageProperty() {
		return html`
			<umb-property-layout
				label=${this.localize.term('user_language')}
				description=${this.localize.term('user_languageHelp')}>
				<umb-ui-culture-input
					slot="editor"
					name="language"
					label=${this.localize.term('user_language')}
					value=${ifDefined(this._languageIsoCode ?? undefined)}
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

export default UmbCurrentUserWorkspaceProfileSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-profile-settings': UmbCurrentUserWorkspaceProfileSettingsElement;
	}
}
