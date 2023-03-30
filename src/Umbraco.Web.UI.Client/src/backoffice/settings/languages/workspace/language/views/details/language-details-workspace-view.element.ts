import { UUIBooleanInputEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbLanguageWorkspaceContext } from '../../language-workspace.context';
import UmbInputCultureSelectElement from '../../../../../../shared/components/input-culture-select/input-culture-select.element';
import UmbInputLanguagePickerElement from '../../../../../../shared/components/input-language-picker/input-language-picker.element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-language-details-workspace-view')
export class UmbLanguageDetailsWorkspaceViewElement extends UmbLitElement {
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

			.validation-message {
				color: var(--uui-color-danger);
			}
		`,
	];

	@state()
	_language?: LanguageResponseModel;

	@state()
	_isDefaultLanguage = false;

	@state()
	_isNew = false;

	@state()
	_validationErrors?: { [key: string]: Array<any> };

	#languageWorkspaceContext?: UmbLanguageWorkspaceContext;

	constructor() {
		super();

		/* TODO: we will need some system to notify about an action has been executed.
		 In the language workspace we want to clear a default language change warning and reset the initial state after a save action has been executed. */
		let initialStateSet = false;

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#languageWorkspaceContext = instance as UmbLanguageWorkspaceContext;

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

			this.observe(this.#languageWorkspaceContext.validationErrors, (value) => {
				this._validationErrors = value;
				this.requestUpdate('_validationErrors');
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

			// to improve UX, we set the name to the culture name if it's a new language
			if (this._isNew && cultureName) {
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
						<!-- TODO: disable already created cultures in the select -->
						<umb-input-culture-select
							value=${ifDefined(this._language.isoCode)}
							@change=${this.#handleCultureChange}
							?readonly=${this._isNew === false}></umb-input-culture-select>

						<!-- TEMP VALIDATION ERROR -->
						${this._validationErrors?.isoCode.map(
							(isoCodeError) => html`<div class="validation-message">${isoCodeError}</div>`
						)}
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
						@change=${this.#handleFallbackChange}
						.filter=${(language: LanguageResponseModel) =>
							language.isoCode !== this._language?.isoCode}></umb-input-language-picker>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbLanguageDetailsWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-details-workspace-view': UmbLanguageDetailsWorkspaceViewElement;
	}
}
