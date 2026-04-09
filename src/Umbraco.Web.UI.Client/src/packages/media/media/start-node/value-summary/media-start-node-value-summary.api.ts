import { UmbMediaItemRepository } from '../../repository/item/media-item.repository.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbValueSummaryApi } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

type StartNode = { unique: string } | null;

export class UmbMediaStartNodeValueSummaryApi
	extends UmbControllerBase
	implements UmbValueSummaryApi<StartNode, UmbMediaItemModel | null>
{
	#repo = new UmbMediaItemRepository(this);

	async resolveValues(values: ReadonlyArray<StartNode>): Promise<ReadonlyArray<UmbMediaItemModel | null>> {
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

export { UmbMediaStartNodeValueSummaryApi as api };
