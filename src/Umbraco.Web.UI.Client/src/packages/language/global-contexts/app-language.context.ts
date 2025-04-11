import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import { UMB_APP_LANGUAGE_CONTEXT } from './app-language.context-token.js';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbReadOnlyStateManager } from '@umbraco-cms/backoffice/utils';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';

// TODO: Make a store for the App Languages.
// TODO: Implement default language end-point, in progress at backend team, so we can avoid getting all languages.
export class UmbAppLanguageContext extends UmbContextBase<UmbAppLanguageContext> implements UmbApi {
	#languagesResolve!: () => void;
	#languagesPromise = new Promise<void>((resolve) => {
		this.#languagesResolve = resolve;
	});
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();
	public readonly cultures = this.#languages.asObservablePart((x) => x.map((y) => y.unique));
	async getCultures() {
		return (await this.observe(this.languages).asPromise()).map((x) => x.unique);
	}

	public readonly appDefaultLanguage = this.#languages.asObservablePart((languages) =>
		languages.find((language) => language.isDefault),
	);

	public readonly moreThanOneLanguage = this.#languages.asObservablePart((x) => x.length > 1);

	#appLanguage = new UmbObjectState<UmbLanguageDetailModel | undefined>(undefined);
	public readonly appLanguage = this.#appLanguage.asObservable();
	public readonly appLanguageCulture = this.#appLanguage.asObservablePart((x) => x?.unique);

	// TODO: I think we should move all read only states to this context and then make a observable regarding the read only state of the app language. [NL]
	public readonly appLanguageReadOnlyState = new UmbReadOnlyStateManager(this);

	public readonly appMandatoryLanguages = this.#languages.asObservablePart((languages) =>
		languages.filter((language) => language.isMandatory),
	);
	async getMandatoryLanguages() {
		await this.#languagesPromise;
		return this.#languages.getValue().filter((language) => language.isMandatory);
	}

	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);
	#currentUserAllowedLanguages: Array<string> = [];
	#currentUserHasAccessToAllLanguages = false;

	#readOnlyStateIdentifier = 'UMB_LANGUAGE_PERMISSION_';
	#localStorageKey = 'umb:appLanguage';

	constructor(host: UmbControllerHost) {
		super(host, UMB_APP_LANGUAGE_CONTEXT);

		// TODO: We need to ensure this request is called every time the user logs in, but this should be done somewhere across the app and not here [JOV]
		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(authContext.isAuthorized, (isAuthorized) => {
				if (!isAuthorized) return;
				this.#requestLanguages();
			});
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.languages, (languages) => {
				this.#currentUserAllowedLanguages = languages || [];
				this.#setIsReadOnly();
			});

			this.observe(context.hasAccessToAllLanguages, (hasAccessToAllLanguages) => {
				this.#currentUserHasAccessToAllLanguages = hasAccessToAllLanguages || false;
				this.#setIsReadOnly();
			});
		});
	}

	getAppCulture() {
		return this.#appLanguage.getValue()?.unique;
	}

	setLanguage(unique: string) {
		// clear the previous read-only state
		const appLanguage = this.#appLanguage.getValue();
		if (appLanguage?.unique) {
			this.appLanguageReadOnlyState.removeState(this.#readOnlyStateIdentifier + appLanguage.unique);
		}

		// find the language
		const language = this.#findLanguage(unique);

		if (!language) {
			throw new Error(`Language with unique ${unique} not found`);
		}

		// set the new language
		this.#appLanguage.update(language);

		// store the new language in local storage
		localStorage.setItem(this.#localStorageKey, language?.unique);

		// set the new read-only state
		this.#setIsReadOnly();
	}

	async #requestLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({});

		// TODO: make this observable / update when languages are added/removed/updated
		if (data) {
			this.#languages.setValue(data.items);
			this.#languagesResolve();

			// If the app language is not set, set it to the default language
			if (!this.#appLanguage.getValue()) {
				this.#initAppLanguage();
			}
		}
	}

	#initAppLanguage() {
		// get the selected language from local storage
		const uniqueFromLocalStorage = localStorage.getItem(this.#localStorageKey);

		if (uniqueFromLocalStorage) {
			const language = this.#findLanguage(uniqueFromLocalStorage);
			if (language) {
				this.setLanguage(language.unique);
				return;
			}
		}

		const defaultLanguage = this.#languages.getValue().find((x) => x.isDefault);
		// TODO: do we always have a default language?
		// do we always get the default language on the first request, or could it be on page 2?
		// in that case do we then need an endpoint to get the default language?
		if (!defaultLanguage?.unique) return;
		this.setLanguage(defaultLanguage.unique);
	}

	#findLanguage(unique: string) {
		return this.#languages.getValue().find((x) => x.unique === unique);
	}

	#setIsReadOnly() {
		const appLanguage = this.#appLanguage.getValue();

		if (!appLanguage) {
			this.appLanguageReadOnlyState.clear();
			return;
		}

		const unique = this.#readOnlyStateIdentifier + appLanguage.unique;
		this.appLanguageReadOnlyState.removeState(unique);

		if (this.#currentUserHasAccessToAllLanguages) {
			return;
		}

		const isReadOnly = !this.#currentUserAllowedLanguages.includes(appLanguage.unique);

		if (isReadOnly) {
			const readOnlyState = {
				unique,
				message: 'You do not have permission to edit to this culture',
			};

			this.appLanguageReadOnlyState.addState(readOnlyState);
		}
	}
}

// Default export to enable this as a globalContext extension js:
export default UmbAppLanguageContext;
