import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageDetails } from '@umbraco-cms/models';
import { UUIBooleanInputEvent, UUIComboboxElement, UUIComboboxEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { repeat } from 'lit-html/directives/repeat.js';
import { customElement, property, state } from 'lit/decorators.js';
import { umbLanguagesData } from 'src/core/mocks/data/languages.data';
import { UmbWorkspaceLanguageContext } from '../../language-workspace.context';

@customElement('umb-workspace-view-language-edit')
export class UmbWorkspaceViewLanguageEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-combobox {
				width: 100%;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
			}
			#default-language-warning {
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border: 1px solid var(--uui-color-warning-standalone);
				margin-top: var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
			}
		`,
	];

	@property()
	language?: LanguageDetails;

	@state()
	private _languages: LanguageDetails[] = [
		{
			id: 1,
			key: 'asdail12h3k1h23k12h3',
			name: 'English',
			isoCode: 'en',
			isDefault: true,
			isMandatory: true,
		},
		{
			id: 2,
			key: 'kajshdkjashdkuahwdu',
			name: 'Danish',
			isoCode: 'da',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
		{
			id: 3,
			key: 'k12n3kj12h3123n9812h3',
			name: 'German',
			isoCode: 'de',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
		{
			id: 4,
			key: '1kl2n31231iuqshdiuashd',
			name: 'French',
			isoCode: 'fr',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
	]; //TODO: This is temporary, we need to get the available languages from the API.

	private _languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (instance) => {
			this._languageWorkspaceContext = instance;

			if (!this._languageWorkspaceContext) return;

			this._languageWorkspaceContext.data.subscribe((language) => {
				this.language = language;
			});
		});
	}

	private _handleLanguageChange(event: any) {
		if (event instanceof UUIComboboxEvent) {
			const target = event.composedPath()[0] as UUIComboboxElement;
			this._languageWorkspaceContext?.update({ isoCode: target.value.toString() });

			// If the language name is not set, we set it to the name of the selected language.
			if (!this.language?.name) {
				const language = this._languages.find((language) => language.isoCode === target.value.toString());
				if (language) {
					this._languageWorkspaceContext?.update({ name: language.name });
				}
			}
		}
	}

	private _handleDefaultChange(event: any) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isDefault: target.checked });
		}
	}

	private _handleMandatoryChange(event: any) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isMandatory: target.checked });
		}
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<uui-box>
				<umb-workspace-property-layout label="Language">
					<uui-combobox slot="editor" value=${this.language.isoCode} @change=${this._handleLanguageChange}>
						<uui-combobox-list>
							${repeat(
								this._languages,
								(language) => language.isoCode,
								(language) =>
									html`
										<uui-combobox-list-option value=${language.isoCode}>${language.name}</uui-combobox-list-option>
									`
							)}
						</uui-combobox-list>
					</uui-combobox>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="ISO Code">
					<div slot="editor">${this.language.isoCode}</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Settings">
					<div slot="editor">
						<uui-toggle ?checked=${this.language.isDefault || false} @change=${this._handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>
						<hr />
						<uui-toggle ?checked=${this.language.isMandatory || false} @change=${this._handleMandatoryChange}>
							<div>
								<b>Mandatory language</b>
								<div>Properties on this language have to be filled out before the node can be published.</div>
							</div>
						</uui-toggle>
						<div id="default-language-warning">Switching default language may result in default content missing.</div>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Fall back language"
					description="To allow multi-lingual content to fall back to another language if not present in the requested language, select it here.">
					<uui-combobox slot="editor">
						<uui-combobox-list>
							<uui-combobox-list-option value="">No fall back language</uui-combobox-list-option>
						</uui-combobox-list>
					</uui-combobox>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewLanguageEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-language-edit': UmbWorkspaceViewLanguageEditElement;
	}
}
