import { UUIBooleanInputEvent, UUIComboboxElement, UUIComboboxEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbWorkspaceLanguageContext } from '../../language-workspace.context';
import {
	UmbLanguageStore,
	UmbLanguageStoreItemType,
	UMB_LANGUAGE_STORE_CONTEXT_TOKEN,
} from '../../../../language.store';
import { UmbLitElement } from '@umbraco-cms/element';
import { Language } from '@umbraco-cms/backend-api';

@customElement('umb-workspace-view-language-edit')
export class UmbWorkspaceViewLanguageEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}
			uui-combobox {
				width: 100%;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
			}
			#culture-warning,
			#default-language-warning {
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border: 1px solid;
				margin-top: var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
			}
			#culture-warning {
				background-color: var(--uui-color-danger);
				color: var(--uui-color-danger-contrast);
				border-color: var(--uui-color-danger-standalone);
			}
			#default-language-warning {
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				border-color: var(--uui-color-warning-standalone);
			}
		`,
	];

	@property()
	language?: UmbLanguageStoreItemType;

	@state()
	private _languages: UmbLanguageStoreItemType[] = [];

	@state()
	private _availableLanguages: UmbLanguageStoreItemType[] = [];

	@state()
	private _search = '';

	@state()
	private _startData: Language | null = null;

	private _languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	constructor() {
		super();

		this.consumeContext<UmbWorkspaceLanguageContext>('umbWorkspaceContext', (instance) => {
			this._languageWorkspaceContext = instance;

			if (!this._languageWorkspaceContext) return;

			this.observe(this._languageWorkspaceContext.data, (language) => {
				this.language = language;

				if (this._startData === null) {
					this._startData = language;
				}
			});
			this.observe(this._languageWorkspaceContext.getAvailableLanguages(), (languages) => {
				this._availableLanguages = languages;
			});
		});

		this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance: UmbLanguageStore) => {
			if (!instance) return;

			instance.getAll().subscribe((languages: Array<UmbLanguageStoreItemType>) => {
				this._languages = languages;
			});
		});
	}

	private _handleLanguageChange(event: Event) {
		if (event instanceof UUIComboboxEvent) {
			const target = event.composedPath()[0] as UUIComboboxElement;
			this._languageWorkspaceContext?.update({ isoCode: target.value.toString() });

			// If the language name is not set, we set it to the name of the selected language.
			if (!this.language?.name) {
				const language = this._availableLanguages.find((language) => language.isoCode === target.value.toString());
				if (language) {
					this._languageWorkspaceContext?.update({ name: language.name });
				}
			}
		}
	}

	private _handleSearchChange(event: Event) {
		const target = event.composedPath()[0] as UUIComboboxElement;
		this._search = target.search;
	}

	private _handleDefaultChange(event: UUIBooleanInputEvent) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isDefault: target.checked });
		}
	}

	private _handleMandatoryChange(event: UUIBooleanInputEvent) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isMandatory: target.checked });
		}
	}

	private _handleFallbackChange(event: UUIComboboxEvent) {
		if (event instanceof UUIComboboxEvent) {
			const target = event.composedPath()[0] as UUIComboboxElement;
			this._languageWorkspaceContext?.update({ fallbackIsoCode: target.value.toString() });
		}
	}

	private get _filteredLanguages(): Array<Language> {
		const onlyNewLanguages = this._availableLanguages.filter(
			(language) =>
				!this._languages.some((x) => x.isoCode === language.isoCode && x.isoCode !== this._startData?.isoCode)
		);

		return onlyNewLanguages.filter((language) => {
			return language.name?.toLowerCase().includes(this._search.toLowerCase());
		});
	}

	private get _fallbackLanguages() {
		return this._languages.filter((language) => {
			return language.isoCode !== this.language?.isoCode;
		});
	}

	private get _fallbackLanguage() {
		return this._fallbackLanguages.find((language) => language.isoCode === this.language?.fallbackIsoCode);
	}

	private get _fromAvailableLanguages() {
		return this._availableLanguages.find((language) => language.isoCode === this.language?.isoCode);
	}

	private _renderCultureWarning() {
		if (this._startData?.isoCode === this.language?.isoCode) return nothing;

		return html`<div id="culture-warning">
			Changing the culture for a language may be an expensive operation and will result in the content cache and indexes
			being rebuilt.
		</div>`;
	}

	private _renderDefaultLanguageWarning() {
		if (this._startData?.isDefault || this.language?.isDefault !== true) return nothing;

		return html`<div id="default-language-warning">
			Switching default language may result in default content missing.
		</div>`;
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<uui-box>
				<umb-workspace-property-layout label="Language">
					<div slot="editor">
						<uui-combobox
							value=${ifDefined(this._fromAvailableLanguages?.isoCode)}
							@change=${this._handleLanguageChange}
							@search=${this._handleSearchChange}>
							<uui-combobox-list>
								${repeat(
									this._filteredLanguages,
									(language) => language.isoCode,
									(language) =>
										html`
											<uui-combobox-list-option value=${ifDefined(language.isoCode)}
												>${language.name}</uui-combobox-list-option
											>
										`
								)}
							</uui-combobox-list>
						</uui-combobox>
						${this._renderCultureWarning()}
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="ISO Code">
					<div slot="editor">${this.language.isoCode}</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Settings">
					<div slot="editor">
						<uui-toggle
							?disabled=${this._startData?.isDefault}
							?checked=${this.language.isDefault || false}
							@change=${this._handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>
						${this._renderDefaultLanguageWarning()}
						<hr />
						<uui-toggle ?checked=${this.language.isMandatory || false} @change=${this._handleMandatoryChange}>
							<div>
								<b>Mandatory language</b>
								<div>Properties on this language have to be filled out before the node can be published.</div>
							</div>
						</uui-toggle>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Fall back language"
					description="To allow multi-lingual content to fall back to another language if not present in the requested language, select it here.">
					<uui-combobox
						slot="editor"
						value=${ifDefined(this._fallbackLanguage?.isoCode)}
						@change=${this._handleFallbackChange}>
						<uui-combobox-list>
							${repeat(
								this._fallbackLanguages,
								(language) => language.isoCode,
								(language) =>
									html`
										<uui-combobox-list-option value=${ifDefined(language.isoCode)}
											>${language.name}</uui-combobox-list-option
										>
									`
							)}
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
