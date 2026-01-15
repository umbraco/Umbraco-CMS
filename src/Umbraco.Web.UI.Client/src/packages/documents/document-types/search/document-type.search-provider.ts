import type { UmbDocumentTypeItemModel } from '../index.js';
import type { UmbDocumentTypeSearchRequestArgs } from './types.js';
import { UmbDocumentTypeSearchRepository } from './document-type-search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';

export interface UmbDocumentTypeSearchItemModel extends UmbDocumentTypeItemModel {
	href: string;
}

export class UmbDocumentTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDocumentTypeSearchItemModel>
{
	#repository = new UmbDocumentTypeSearchRepository(this);

	async search(args: UmbDocumentTypeSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentTypeSearchProvider as api };
