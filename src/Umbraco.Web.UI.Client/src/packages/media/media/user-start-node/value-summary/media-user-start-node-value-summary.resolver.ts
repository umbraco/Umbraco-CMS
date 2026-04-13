import { UmbMediaItemRepository } from '../../repository/item/media-item.repository.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

type StartNode = { unique: string } | null;

export class UmbMediaUserStartNodeValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<StartNode, UmbMediaItemModel | null>
{
	#repo = new UmbMediaItemRepository(this);

	async resolveValues(values: ReadonlyArray<StartNode>): Promise<UmbValueSummaryResolveResult<UmbMediaItemModel | null>> {
		const uniques = [...new Set(values.filter(Boolean).map((v) => v!.unique))];

		if (uniques.length === 0) {
			return { data: values.map(() => null) };
		}

		const { data, asObservable } = await this.#repo.requestItems(uniques);
		const items = Array.isArray(data) ? data : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<StartNode>,
		items: ReadonlyArray<UmbMediaItemModel>,
	): ReadonlyArray<UmbMediaItemModel | null> {
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) => (v ? (itemByUnique.get(v.unique) ?? null) : null));
	}
}

// Named 'api' for ApiLoaderProperty convention
export { UmbMediaUserStartNodeValueSummaryResolver as api };
