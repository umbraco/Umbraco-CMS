import { UmbDocumentItemRepository } from '../../item/repository/document-item.repository.js';
import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

type StartNode = { unique: string } | null;

export class UmbDocumentStartNodeValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<StartNode, UmbDocumentItemModel | null>
{
	#repo = new UmbDocumentItemRepository(this);

	async resolveValues(values: ReadonlyArray<StartNode>): Promise<ReadonlyArray<UmbDocumentItemModel | null>> {
		const uniques = [...new Set(values.filter(Boolean).map((v) => v!.unique))];

		if (uniques.length === 0) {
			return values.map(() => null);
		}

		const { data } = await this.#repo.requestItems(uniques);
		const items = Array.isArray(data) ? data : [];
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));

		return values.map((v) => (v ? (itemByUnique.get(v.unique) ?? null) : null));
	}
}

// Named 'api' for ApiLoaderProperty convention
export { UmbDocumentStartNodeValueSummaryResolver as api };
