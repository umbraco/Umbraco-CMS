import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context.js';
import { html, customElement, state, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-user-workspace-profile-settings')
export class UmbUserWorkspaceProfileSettingsElement extends UmbLitElement {
	@state()
	private _user?: UserResponseModel;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(this.#userWorkspaceContext.data, (user) => (this._user = user), 'umbUserObserver');
		});
	}

	#onLanguageChange(event: UmbChangeEvent) {
		const target = event.target as UmbUiCultureInputElement;

		if (typeof target?.value === 'string') {
			this.#userWorkspaceContext?.updateProperty('languageIsoCode', target.value);
		}
	}

	render() {
		return html`<uui-box>
			<div slot="headline"><umb-localize key="user_profile">Profile</umb-localize></div>
			${this.#renderEmailProperty()} ${this.#renderUILanguageProperty()}
		</uui-box>`;
	}

	#renderEmailProperty() {
		return html`
			<umb-property-layout label="${this.localize.term('general_email')}">
				<uui-input
					slot="editor"
					name="email"
					label="${this.localize.term('general_email')}"
					readonly
					value=${ifDefined(this._user?.email)}></uui-input>
			</umb-property-layout>
		`;
	}

	#renderUILanguageProperty() {
		return html`
			<umb-property-layout
				label="${this.localize.term('user_language')}"
				description=${this.localize.term('user_languageHelp')}>
				<umb-ui-culture-input
					slot="editor"
					value=${ifDefined(this._user?.languageIsoCode)}
					@change="${this.#onLanguageChange}"
					name="language"
					label="${this.localize.term('user_language')}"></umb-ui-culture-input>
			</umb-property-layout>
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
