import { UmbLanguageRepository } from '../repository/language.repository';
import { ObjectState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbAppLanguageContext {
	#host: UmbControllerHostElement;
	#languageRepository: UmbLanguageRepository;

	#languages: Array<LanguageResponseModel> = [];

	#appLanguage = new ObjectState<LanguageResponseModel | undefined>(undefined);
	appLanguage = this.#appLanguage.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#languageRepository = new UmbLanguageRepository(this.#host);
		this.#observeLanguages();
	}

	setLanguage(isoCode: string) {
		const language = this.#languages.find((x) => x.isoCode === isoCode);
		this.#appLanguage.update(language);
	}

	async #observeLanguages() {
		const { asObservable } = await this.#languageRepository.requestLanguages();

		new UmbObserverController(this.#host, asObservable(), (languages) => {
			this.#languages = languages;

			// If the app language is not set, set it to the default language
			if (!this.#appLanguage.getValue()) {
				this.#initAppLanguage();
			}
		});
	}

	#initAppLanguage() {
		const defaultLanguage = this.#languages.find((x) => x.isDefault);
		// TODO: do we always have a default language?
		// do we always get the default language on the first request, or could it be on page 2?
		// in that case do we then need an endpoint to get the default language?
		if (!defaultLanguage?.isoCode) return;
		this.setLanguage(defaultLanguage.isoCode);
	}
}

export const UMB_APP_LANGUAGE_CONTEXT_TOKEN = new UmbContextToken<UmbAppLanguageContext>('UmbAppLanguageContext');
