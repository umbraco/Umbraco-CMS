import { UUIBooleanInputEvent, UUIComboboxElement, UUIComboboxEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLanguageWorkspaceContext } from '../../language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageModel } from '@umbraco-cms/backend-api';
import { UmbChangeEvent } from 'src/core/events';
import UmbInputCultureSelectElement from 'src/backoffice/shared/components/input-culture-select/input-culture-select.element';

@customElement('umb-edit-language-workspace-view')
export class UmbEditLanguageWorkspaceViewElement extends UmbLitElement {
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
	language?: LanguageModel;

	@state()
	private _languages: LanguageModel[] = [];

	@state()
	private _startData: LanguageModel | null = null;

	#languageWorkspaceContext?: UmbLanguageWorkspaceContext;

	constructor() {
		super();

		this.consumeContext<UmbLanguageWorkspaceContext>('umbWorkspaceContext', (instance) => {
			this.#languageWorkspaceContext = instance;

			this.observe(this.#languageWorkspaceContext.data, (language) => {
				this.language = language;
			});
		});
	}

	#handleCultureChange(event: Event) {
		if (event instanceof UmbChangeEvent) {
			const target = event.target as UmbInputCultureSelectElement;
			const isoCode = target.value.toString();
			const cultureName = target.selectedCultureName;

			if (!isoCode) {
				// If the isoCode is empty, we reset the value to the original value.
				// Provides a way better UX
				//TODO: Maybe the combobox should implement something similar?
				const resetFunction = () => {
					target.value = this.language?.isoCode ?? '';
				};

				target.addEventListener('close', resetFunction, { once: true });
				target.addEventListener('blur', resetFunction, { once: true });
				return;
			}

			this.#languageWorkspaceContext?.setCulture(isoCode);

			// If the language name is not set, we set it to the name of the selected language.
			if (!this.language?.name && cultureName) {
				this.#languageWorkspaceContext?.setName(cultureName);
			}
		}
	}

	#handleDefaultChange(event: UUIBooleanInputEvent) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;
			this.#languageWorkspaceContext?.setDefault(target.checked);
		}
	}

	#handleMandatoryChange(event: UUIBooleanInputEvent) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;
			this.#languageWorkspaceContext?.setMandatory(target.checked);
		}
	}

	#handleFallbackChange(event: UUIComboboxEvent) {
		if (event instanceof UUIComboboxEvent) {
			const target = event.composedPath()[0] as UUIComboboxElement;
			this.#languageWorkspaceContext?.setFallbackCulture(target.value.toString());
		}
	}

	get #fallbackLanguages() {
		return this._languages.filter((language) => {
			return language.isoCode !== this.language?.isoCode;
		});
	}

	get #fallbackLanguage() {
		return this.#fallbackLanguages.find((language) => language.isoCode === this.language?.fallbackIsoCode);
	}

	#renderCultureWarning() {
		if (!this._startData?.isoCode || this._startData?.isoCode === this.language?.isoCode) return nothing;

		return html`<div id="culture-warning">
			Changing the culture for a language may be an expensive operation and will result in the content cache and indexes
			being rebuilt.
		</div>`;
	}

	#renderDefaultLanguageWarning() {
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
						<umb-input-culture-select
							value=${ifDefined(this.language.isoCode)}
							@change=${this.#handleCultureChange}></umb-input-culture-select>
						${this.#renderCultureWarning()}
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
							@change=${this.#handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>
						${this.#renderDefaultLanguageWarning()}
						<hr />
						<uui-toggle ?checked=${this.language.isMandatory || false} @change=${this.#handleMandatoryChange}>
							<div>
								<b>Mandatory language</b>
								<div>Properties on this language have to be filled out before the node can be published.</div>
							</div>
						</uui-toggle>
					</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout
					label="Fallback language"
					description="To allow multi-lingual content to fall back to another language if not present in the requested language, select it here.">
					<umb-input-language-picker slot="editor"></umb-input-language-picker>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbEditLanguageWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-edit-language-workspace-view': UmbEditLanguageWorkspaceViewElement;
	}
}
