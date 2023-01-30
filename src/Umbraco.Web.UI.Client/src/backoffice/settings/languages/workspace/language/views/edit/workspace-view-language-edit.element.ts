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
	language?: UmbLanguageStoreItemType;

	@state()
	private _languages: UmbLanguageStoreItemType[] = [];

	@state()
	private _search = '';

	private _languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	constructor() {
		super();

		this.consumeContext<UmbWorkspaceLanguageContext>('umbWorkspaceContext', (instance) => {
			this._languageWorkspaceContext = instance;

			if (!this._languageWorkspaceContext) return;

			this._languageWorkspaceContext.data.subscribe((language) => {
				this.language = language;
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
				const language = this._languages.find((language) => language.isoCode === target.value.toString());
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

	private get _filteredLanguages() {
		return this._languages.filter((language) => {
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

	private _renderDefaultLanguageWarning() {
		let originalIsDefault = false;

		if (this.language?.isoCode) {
			originalIsDefault =
				this._languages.find((language) => language.isoCode === this.language?.isoCode)?.isDefault ?? false;
		}

		if (originalIsDefault === this.language?.isDefault) return nothing;

		return html`<div id="default-language-warning">
			Switching default language may result in default content missing.
		</div>`;
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<uui-box>
				<umb-workspace-property-layout label="Language">
					<uui-combobox
						slot="editor"
						value=${ifDefined(this.language.isoCode)}
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
						${this._renderDefaultLanguageWarning()}
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
