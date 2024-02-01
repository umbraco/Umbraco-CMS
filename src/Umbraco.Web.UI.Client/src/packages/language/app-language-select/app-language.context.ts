import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbAppLanguageContext extends UmbBaseController implements UmbApi {
	#languageCollectionRepository: UmbLanguageCollectionRepository;
	#languages: Array<UmbLanguageDetailModel> = [];
	#appLanguage = new UmbObjectState<UmbLanguageDetailModel | undefined>(undefined);
	appLanguage = this.#appLanguage.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_APP_LANGUAGE_CONTEXT, this);
		this.#languageCollectionRepository = new UmbLanguageCollectionRepository(this);
		this.#observeLanguages();
	}

	setLanguage(unique: string) {
		const language = this.#languages.find((x) => x.unique === unique);
		this.#appLanguage.update(language);
	}

	async #observeLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({ skip: 0, take: 100 });

		// TODO: make this observable / update when languages are added/removed/updated
		if (data) {
			this.#languages = data.items;

			// If the app language is not set, set it to the default language
			if (!this.#appLanguage.getValue()) {
				this.#initAppLanguage();
			}
		}
	}

	#initAppLanguage() {
		const defaultLanguage = this.#languages.find((x) => x.isDefault);
		// TODO: do we always have a default language?
		// do we always get the default language on the first request, or could it be on page 2?
		// in that case do we then need an endpoint to get the default language?
		if (!defaultLanguage?.unique) return;
		this.setLanguage(defaultLanguage.unique);
	}
}

// Default export to enable this as a globalContext extension js:
export default UmbAppLanguageContext;

export const UMB_APP_LANGUAGE_CONTEXT = new UmbContextToken<UmbAppLanguageContext>('UmbAppLanguageContext');
