import { UMB_CURRENT_USER_WORKSPACE_CONTEXT } from './current-user-workspace.context-token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbUiCultureInputElement } from '@umbraco-cms/backoffice/localization';

@customElement('umb-current-user-workspace-settings')
export class UmbCurrentUserWorkspaceSettingsElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_CURRENT_USER_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _languageIsoCode = '';

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			if (!context) return;

			this.observe(
				context.languageIsoCode,
				(code) => (this._languageIsoCode = code ?? ''),
				'umbCurrentUserWorkspaceLanguageObserver',
			);
		});
	}

	#onLanguageChange(event: UmbChangeEvent & { target: UmbUiCultureInputElement }) {
		const value = event.target?.value;
		if (typeof value === 'string') {
			this.#workspaceContext?.setLanguageIsoCode(value);
		}
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

export default UmbCurrentUserWorkspaceSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-settings': UmbCurrentUserWorkspaceSettingsElement;
	}
}
