import type { UmbDocumentTypeItemModel } from '../index.js';
import { UmbDocumentTypeSearchRepository } from './document-type-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbDocumentTypeSearchItemModel extends UmbDocumentTypeItemModel {
	href: string;
}

export class UmbDocumentTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDocumentTypeSearchItemModel>
{
	#repository = new UmbDocumentTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentTypeSearchProvider as api };
