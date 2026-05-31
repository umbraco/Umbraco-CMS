import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '@umbraco-cms/backoffice/document';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentLinkPickerContext extends UmbPickerContext {
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public languages = this.#languages.asObservable();

	#culture = new UmbStringState<string | null>(null);
	public culture = this.#culture.asObservable();

	#variantContext?: UmbVariantContext;
	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.search.setConfig({
			providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		});

		this.#loadLanguages();
	}

	async setCulture(culture: string | null) {
		this.#culture.setValue(culture);
		await this.#variantContext?.setCulture(culture);
	}

	async getCulture(): Promise<string | null> {
		return this.#culture.getValue();
	}

	async #loadLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({ skip: 0, take: 1000 });
		const languages = data?.items || [];

		this.#languages.setValue(languages);

		if (languages.length > 0) {
			// Create variant context only when we have languages available
			this.#variantContext = new UmbVariantContext(this).inherit();
		} else {
			// No languages available - ensure variant context is not set
			this.#variantContext?.destroy();
			this.#variantContext = undefined;
		}
	}
}

export { UmbDocumentLinkPickerContext as api };
