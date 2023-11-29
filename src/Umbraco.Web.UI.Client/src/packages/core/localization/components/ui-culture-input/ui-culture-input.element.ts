import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

interface UmbCultureInputOption {
	name: string;
	value: string;
	selected: boolean;
}

@customElement('umb-ui-culture-input')
export class UmbUiCultureInputElement extends FormControlMixin(UmbLitElement) {
	@state()
	private _cultures: Array<UmbCultureInputOption> = [];

	@query('uui-select')
	private _selectElement!: HTMLInputElement;

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			this.#observeCurrentUser();
		});
	}

	#observeCurrentUser() {
		if (!this.#currentUserContext) return;

		this.observe(
			this.#currentUserContext.currentUser,
			async (currentUser) => {
				if (!currentUser) {
					return;
				}

				// Find all translations and make a unique list of iso codes
				const translations = await firstValueFrom(umbExtensionsRegistry.extensionsOfType('localization'));

				this._cultures = translations
					.filter((isoCode) => isoCode !== undefined)
					.map((translation) => ({
						value: translation.meta.culture.toLowerCase(),
						name: translation.name,
						selected: false,
					}));

				const currentUserLanguageCode = currentUser.languageIsoCode?.toLowerCase();

				// Set the current user's language as selected
				const currentUserLanguage = this._cultures.find((language) => language.value === currentUserLanguageCode);

				if (currentUserLanguage) {
					currentUserLanguage.selected = true;
				} else {
					// If users language code did not fit any of the options. We will create an option that fits, named unknown.
					// In this way the user can keep their choice though a given language was not present at this time.
					this._cultures.push({
						value: currentUserLanguageCode ?? 'en-us',
						name: currentUserLanguageCode ? `${currentUserLanguageCode} (unknown)` : 'Unknown',
						selected: true,
					});
				}
			},
			'umbUserObserver',
		);
	}

	protected getFormElement() {
		return this._selectElement;
	}

	#onChange(event: Event) {
		event.stopPropagation();
		const target = event.composedPath()[0] as UUISelectElement;

		if (typeof target?.value === 'string') {
			this.value = target.value;
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	render() {
		return html` <uui-select .options=${this._cultures} @change="${this.#onChange}"> </uui-select>`;
	}

	static styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbUiCultureInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ui-culture-input': UmbUiCultureInputElement;
	}
}
