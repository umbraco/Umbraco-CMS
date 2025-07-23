import { UmbUserKind } from '../../../../utils/index.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type { UmbUserDetailModel } from '../../../../types.js';
import { html, customElement, state, ifDefined, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-user-workspace-profile-settings')
export class UmbUserWorkspaceProfileSettingsElement extends UmbLitElement {
	@state()
	private _user?: UmbUserDetailModel;

	@state()
	private _usernameIsEmail = true;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(this.#userWorkspaceContext?.data, (user) => (this._user = user), 'umbUserObserver');
			this.observe(
				this.#userWorkspaceContext?.configRepository.part('usernameIsEmail'),
				(usernameIsEmail) => (this._usernameIsEmail = usernameIsEmail === undefined ? true : usernameIsEmail),
				'umbUsernameIsEmailObserver',
			);
		});
	}

	#onEmailChange(event: UmbChangeEvent) {
		const target = event.target as HTMLInputElement;

		if (typeof target?.value === 'string') {
			this.#userWorkspaceContext?.updateProperty('email', target.value);

			if (this._usernameIsEmail) {
				this.#userWorkspaceContext?.updateProperty('userName', target.value);
			}
		}
	}

	#onUsernameChange(event: UmbChangeEvent) {
		const target = event.target as HTMLInputElement;

		if (typeof target?.value === 'string') {
			this.#userWorkspaceContext?.updateProperty('userName', target.value);
		}
	}

	#onLanguageChange(event: UmbChangeEvent) {
		const target = event.target as UmbUiCultureInputElement;

		if (typeof target?.value === 'string') {
			this.#userWorkspaceContext?.updateProperty('languageIsoCode', target.value);
		}
	}

	override render() {
		return html`
			<uui-box>
				<div slot="headline"><umb-localize key="user_profile">Profile</umb-localize></div>
				${this.#renderEmailProperty()} ${this.#renderUsernameProperty()} ${this.#renderUILanguageProperty()}
			</uui-box>
		`;
	}

	#renderEmailProperty() {
		return html`
			<umb-property-layout
				mandatory
				label=${this.localize.term('general_email')}
				description=${this.localize.term('user_emailDescription', this._usernameIsEmail)}>
				<uui-input
					slot="editor"
					name="email"
					label=${this.localize.term('general_email')}
					required
					required-message=${this.localize.term('user_emailRequired')}
					value=${ifDefined(this._user?.email)}
					@change=${this.#onEmailChange}
					${umbBindToValidation(this)}>
				</uui-input>
			</umb-property-layout>
		`;
	}

	#renderUsernameProperty() {
		if (this._usernameIsEmail) return nothing;

		return html`
			<umb-property-layout
				mandatory
				label=${this.localize.term('user_loginname')}
				description=${this.localize.term('user_loginnameDescription')}>
				<uui-input
					slot="editor"
					name="username"
					autocomplete="off"
					label=${this.localize.term('user_loginname')}
					required
					required-message=${this.localize.term('user_loginnameRequired')}
					value=${ifDefined(this._user?.userName)}
					@change=${this.#onUsernameChange}>
				</uui-input>
			</umb-property-layout>
		`;
	}

	#renderUILanguageProperty() {
		if (this._user?.kind === UmbUserKind.API) return nothing;
		return html`
			<umb-property-layout
				label=${this.localize.term('user_language')}
				description=${this.localize.term('user_languageHelp')}>
				<umb-ui-culture-input
					slot="editor"
					name="language"
					label=${this.localize.term('user_language')}
					value=${ifDefined(this._user?.languageIsoCode ?? undefined)}
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
