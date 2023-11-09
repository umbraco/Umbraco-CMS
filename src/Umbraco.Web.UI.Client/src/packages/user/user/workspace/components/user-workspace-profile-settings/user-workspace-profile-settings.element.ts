import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context.js';
import { html, customElement, state, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_AUTH_CONTEXT, UmbCurrentUser } from '@umbraco-cms/backoffice/auth';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-user-workspace-profile-settings')
export class UmbUserWorkspaceProfileSettingsElement extends UmbLitElement {
	@state()
	private _user?: UserResponseModel;

	@state()
	private _currentUser?: UmbCurrentUser;

	@state()
	private languages: Array<{ name: string; value: string; selected: boolean }> = [];

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_AUTH_CONTEXT, (instance) => {
			this.#authContext = instance;
			this.#observeCurrentUser();
		});

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(this.#userWorkspaceContext.data, (user) => (this._user = user), 'umbUserObserver');
		});
	}

	#onLanguageChange(event: Event) {
		const target = event.composedPath()[0] as UUISelectElement;

		if (typeof target?.value === 'string') {
			this.#userWorkspaceContext?.updateProperty('languageIsoCode', target.value);
		}
	}

	#observeCurrentUser() {
		if (!this.#authContext) return;
		this.observe(
			this.#authContext.currentUser,
			async (currentUser) => {
				this._currentUser = currentUser;

				if (!currentUser) {
					return;
				}

				// Find all translations and make a unique list of iso codes
				const translations = await firstValueFrom(umbExtensionsRegistry.extensionsOfType('localization'));

				this.languages = translations
					.filter((isoCode) => isoCode !== undefined)
					.map((translation) => ({
						value: translation.meta.culture.toLowerCase(),
						name: translation.name,
						selected: false,
					}));

				const currentUserLanguageCode = currentUser.languageIsoCode?.toLowerCase();

				// Set the current user's language as selected
				const currentUserLanguage = this.languages.find((language) => language.value === currentUserLanguageCode);

				if (currentUserLanguage) {
					currentUserLanguage.selected = true;
				} else {
					// If users language code did not fit any of the options. We will create an option that fits, named unknown.
					// In this way the user can keep their choice though a given language was not present at this time.
					this.languages.push({
						value: currentUserLanguageCode ?? 'en-us',
						name: currentUserLanguageCode ? `${currentUserLanguageCode} (unknown)` : 'Unknown',
						selected: true,
					});
				}
			},
			'umbUserObserver',
		);
	}

	render() {
		return html`<uui-box>
			<div slot="headline"><umb-localize key="user_profile">Profile</umb-localize></div>
			${this.#renderEmailProperty()} ${this.#renderUILanguageProperty()}
		</uui-box>`;
	}

	#renderEmailProperty() {
		return html`
			<umb-workspace-property-layout label="${this.localize.term('general_email')}">
				<uui-input
					slot="editor"
					name="email"
					label="${this.localize.term('general_email')}"
					readonly
					value=${ifDefined(this._user?.email)}></uui-input>
			</umb-workspace-property-layout>
		`;
	}

	#renderUILanguageProperty() {
		return html`
			<umb-workspace-property-layout
				label="${this.localize.term('user_language')}"
				description=${this.localize.term('user_languageHelp')}>
				<uui-select
					slot="editor"
					name="language"
					label="${this.localize.term('user_language')}"
					.options=${this.languages}
					@change="${this.#onLanguageChange}">
				</uui-select>
			</umb-workspace-property-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbUserWorkspaceProfileSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-profile-settings': UmbUserWorkspaceProfileSettingsElement;
	}
}
