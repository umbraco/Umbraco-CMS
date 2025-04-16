import { UMB_LANGUAGE_WORKSPACE_CONTEXT } from '../language-workspace.context-token.js';
import type { UmbInputLanguageElement } from '../../../components/input-language/input-language.element.js';
import type { UmbLanguageDetailModel, UmbLanguageItemModel } from '../../../types.js';
import type { UmbInputCultureSelectElement } from '@umbraco-cms/backoffice/culture';
import type { UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';

@customElement('umb-language-details-workspace-view')
export class UmbLanguageDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	_language?: UmbLanguageDetailModel;

	@state()
	_isDefaultLanguage = false;

	@state()
	_isNew?: boolean;

	#languageWorkspaceContext?: typeof UMB_LANGUAGE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		/* TODO: we will need some system to notify about an action has been executed.
		 In the language workspace we want to clear a default language change warning and reset the initial state after a save action has been executed. */
		let initialStateSet = false;

		this.consumeContext(UMB_LANGUAGE_WORKSPACE_CONTEXT, (instance) => {
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

			this.observe(this.#languageWorkspaceContext.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#handleCultureChange(event: Event) {
		if (event instanceof UmbChangeEvent) {
			const target = event.target as UmbInputCultureSelectElement;
			const unique = target.value.toString();
			const cultureName = target.selectedCultureName;

			// If there is no cultureName, it was probably an unknown event that triggered the change event, so ignore it.
			if (!cultureName) {
				return;
			}

			if (!unique) {
				// If the unique is empty, we reset the value to the original value.
				// Provides a way better UX
				//TODO: Maybe the combobox should implement something similar?
				const resetFunction = () => (target.value = this._language?.unique as string);

				target.addEventListener('close', resetFunction, { once: true });
				target.addEventListener('blur', resetFunction, { once: true });
				return;
			}

			this.#languageWorkspaceContext?.setCulture(unique);

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
			const target = event.target as UmbInputLanguageElement;
			const selectedLanguageUnique = target.selection?.[0];
			this.#languageWorkspaceContext?.setFallbackCulture(selectedLanguageUnique);
		}
	}

	override render() {
		return html`
			<uui-box>
				<umb-property-layout label="Language">
					<div slot="editor">
						<!-- TODO: disable already created cultures in the select -->
						${this._isNew
							? html` <umb-input-culture-select
									value=${ifDefined(this._language?.unique)}
									@change=${this.#handleCultureChange}></umb-input-culture-select>`
							: this._language?.name}
					</div>
				</umb-property-layout>

				<umb-property-layout label="ISO Code">
					<div slot="editor">${this._language?.unique}</div>
				</umb-property-layout>

				<umb-property-layout label="Settings">
					<div slot="editor">
						<uui-toggle
							label="Default language"
							?disabled=${this._isDefaultLanguage}
							?checked=${this._language?.isDefault || false}
							@change=${this.#handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>
						<!-- 	TODO: we need a UUI component for this -->
						${this._language?.isDefault && this._language?.isDefault !== this._isDefaultLanguage
							? html`<div id="default-language-warning">
									Switching default language may result in default content missing.
								</div>`
							: nothing}

						<hr />
						<uui-toggle
							label="Mandatory language"
							?checked=${this._language?.isMandatory || false}
							@change=${this.#handleMandatoryChange}>
							<div>
								<b>Mandatory language</b>
								<div>Properties on this language have to be filled out before the node can be published.</div>
							</div>
						</uui-toggle>
					</div>
				</umb-property-layout>

				<umb-property-layout
					label="Fallback language"
					description="To allow multi-lingual content to fall back to another language if not present in the requested language, select it here.">
					<umb-input-language
						value=${ifDefined(this._language?.fallbackIsoCode === null ? undefined : this._language?.fallbackIsoCode)}
						slot="editor"
						max="1"
						@change=${this.#handleFallbackChange}
						.filter=${(language: UmbLanguageItemModel) =>
							language.unique !== this._language?.unique}></umb-input-language>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
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
}

export default UmbLanguageDetailsWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-details-workspace-view': UmbLanguageDetailsWorkspaceViewElement;
	}
}
