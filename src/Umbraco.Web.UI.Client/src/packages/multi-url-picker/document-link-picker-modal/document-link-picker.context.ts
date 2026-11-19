import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '@umbraco-cms/backoffice/document';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import { UmbTreeItemPickerExpansionManager } from '@umbraco-cms/backoffice/tree';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentLinkPickerContext extends UmbPickerContext {
	public readonly expansion = new UmbTreeItemPickerExpansionManager(this, {
		interactionMemoryManager: this.interactionMemory,
	});

	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public languages = this.#languages.asObservable();

	// Provided downward (so the tree renders document names in the picked culture) and read from directly
	// here to scope search - both need the same "explicit pick, else inherited" culture, so one context
	// computes it once rather than each maintaining its own copy.
	#variantContext = new UmbVariantContext(this).inherit();
	public culture = this.#variantContext.culture;

	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.search.setConfig({
			providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		});

		this.observe(this.#variantContext.displayCulture, (culture) => this.#updateSearchCulture(culture ?? null), null);

		this.#loadLanguages();
	}

	async setCulture(culture: string | null) {
		await this.#variantContext.setCulture(culture);
	}

	async getCulture(): Promise<string | null> {
		return (await this.#variantContext.getCulture()) ?? null;
	}

	#updateSearchCulture(culture: string | null) {
		this.search.updateConfig({ queryParams: { culture } });

		// Re-run an already active search so visible results reflect the new culture scope right away.
		if (this.search.getSearchable() && this.search.getQuery()?.query) {
			this.search.search();
		}
	}

	async #loadLanguages() {
		const { data } = await this.#languageCollectionRepository.requestAllItems();
		this.#languages.setValue(data?.items || []);
	}
}

export { UmbDocumentLinkPickerContext as api };
