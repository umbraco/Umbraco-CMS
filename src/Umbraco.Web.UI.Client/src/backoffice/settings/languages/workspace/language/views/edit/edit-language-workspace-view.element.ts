import { UUIBooleanInputEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLanguageWorkspaceContext } from '../../language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageModel } from '@umbraco-cms/backend-api';
import { UmbChangeEvent } from '@umbraco-cms/events';
import UmbInputCultureSelectElement from 'src/backoffice/shared/components/input-culture-select/input-culture-select.element';
import UmbInputLanguagePickerElement from 'src/backoffice/shared/components/input-language-picker/input-language-picker.element';

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

			#default-language-warning {
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				border-color: var(--uui-color-warning-standalone);
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border: 1px solid;
				margin-top: var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
			}
		`,
	];

	@state()
	_language?: LanguageModel;

	@state()
	_isDefaultLanguage = false;

	@state()
	_isNew = false;

	#languageWorkspaceContext?: UmbLanguageWorkspaceContext;

	constructor() {
		super();

		let initialStateSet = false;

		this.consumeContext<UmbLanguageWorkspaceContext>('umbWorkspaceContext', (instance) => {
			this.#languageWorkspaceContext = instance;

			this.observe(this.#languageWorkspaceContext.data, (language) => {
				this._language = language;

				/* Store the initial value of the default language.
				 When we change the default language we get notified of the change,
				and we need the initial value to compare against */
				if (initialStateSet === false) {
					this._isDefaultLanguage = language?.isDefault ?? false;
					initialStateSet = true;
				}
			});

			this.observe(this.#languageWorkspaceContext.isNew, (value) => {
				this._isNew = value;
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
					target.value = this._language?.isoCode ?? '';
				};

				target.addEventListener('close', resetFunction, { once: true });
				target.addEventListener('blur', resetFunction, { once: true });
				return;
			}

			this.#languageWorkspaceContext?.setCulture(isoCode);

			// If the language name is not set, we set it to the name of the selected language.
			if (!this._language?.name && cultureName) {
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

	#handleFallbackChange(event: UmbChangeEvent) {
		if (event instanceof UmbChangeEvent) {
			const target = event.target as UmbInputLanguagePickerElement;
			const selectedLanguageIsoCode = target.selectedIsoCodes?.[0];
			this.#languageWorkspaceContext?.setFallbackCulture(selectedLanguageIsoCode);
		}
	}

	render() {
		if (!this._language) return nothing;

		return html`
			<uui-box>
				<umb-workspace-property-layout label="Language">
					<div slot="editor">
						<umb-input-culture-select
							value=${ifDefined(this._language.isoCode)}
							@change=${this.#handleCultureChange}
							?readonly=${this._isNew === false}></umb-input-culture-select>
					</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout label="ISO Code">
					<div slot="editor">${this._language.isoCode}</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout label="Settings">
					<div slot="editor">
						<uui-toggle
							?disabled=${this._isDefaultLanguage}
							?checked=${this._language.isDefault || false}
							@change=${this.#handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>

						<!-- 	TODO: we need a UUI component for this -->
						${this._language.isDefault !== this._isDefaultLanguage
							? html`<div id="default-language-warning">
									Switching default language may result in default content missing.
							  </div>`
							: nothing}

						<hr />
						<uui-toggle ?checked=${this._language.isMandatory || false} @change=${this.#handleMandatoryChange}>
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
					<umb-input-language-picker
						value=${ifDefined(this._language.fallbackIsoCode === null ? undefined : this._language.fallbackIsoCode)}
						slot="editor"
						max="1"
						@change=${this.#handleFallbackChange}></umb-input-language-picker>
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
