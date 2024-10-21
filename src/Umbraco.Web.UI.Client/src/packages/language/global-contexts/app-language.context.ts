import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import { UMB_APP_LANGUAGE_CONTEXT } from './app-language.context-token.js';
import { UmbArrayState, UmbObjectState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbReadOnlyStateManager } from '@umbraco-cms/backoffice/utils';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

// TODO: Make a store for the App Languages.
// TODO: Implement default language end-point, in progress at backend team, so we can avoid getting all languages.
export class UmbAppLanguageContext extends UmbContextBase<UmbAppLanguageContext> implements UmbApi {
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);

	#appLanguage = new UmbObjectState<UmbLanguageDetailModel | undefined>(undefined);
	public readonly appLanguage = this.#appLanguage.asObservable();
	public readonly appLanguageCulture = this.#appLanguage.asObservablePart((x) => x?.unique);

	public readonly appLanguageReadOnlyState = new UmbReadOnlyStateManager(this);

	public readonly appDefaultLanguage = createObservablePart(this.#languages.asObservable(), (languages) =>
		languages.find((language) => language.isDefault),
	);

	public readonly moreThanOneLanguage = this.#languages.asObservablePart((x) => x.length > 1);

	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);
	#currentUserAllowedLanguages: Array<string> = [];
	#currentUserHasAccessToAllLanguages = false;

	#readOnlyStateIdentifier = 'UMB_LANGUAGE_PERMISSION_';

	constructor(host: UmbControllerHost) {
		super(host, UMB_APP_LANGUAGE_CONTEXT);

		// TODO: We need to ensure this request is called every time the user logs in, but this should be done somewhere across the app and not here [JOV]
		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(authContext.isAuthorized, (isAuthorized) => {
				if (!isAuthorized) return;
				this.#observeLanguages();
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
		const appLanguage = this.#appLanguage.getValue();

		// clear the previous read-only state
		if (appLanguage?.unique) {
			this.appLanguageReadOnlyState.removeState(this.#readOnlyStateIdentifier + appLanguage.unique);
		}

		// set the new language
		const languages = this.#languages.getValue();
		const language = languages.find((x) => x.unique === unique);
		this.#appLanguage.update(language);

		// set the new read-only state
		this.#setIsReadOnly();
	}

	async #observeLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({});

		// TODO: make this observable / update when languages are added/removed/updated
		if (data) {
			this.#languages.setValue(data.items);

			// If the app language is not set, set it to the default language
			if (!this.#appLanguage.getValue()) {
				this.#initAppLanguage();
			}
		}
	}

	#initAppLanguage() {
		const defaultLanguage = this.#languages.getValue().find((x) => x.isDefault);
		// TODO: do we always have a default language?
		// do we always get the default language on the first request, or could it be on page 2?
		// in that case do we then need an endpoint to get the default language?
		if (!defaultLanguage?.unique) return;
		this.setLanguage(defaultLanguage.unique);
		this.#setIsReadOnly();
	}

	#setIsReadOnly() {
		const appLanguage = this.#appLanguage.getValue();

		if (!appLanguage) {
			this.appLanguageReadOnlyState.clear();
			return;
		}

		const unique = this.#readOnlyStateIdentifier + appLanguage.unique;
		this.appLanguageReadOnlyState.removeState(unique);

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
