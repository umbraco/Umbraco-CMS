import { UmbUserGroupItemRepository } from '../repository/item/user-group-item.repository.js';
import type { UmbUserGroupItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserGroupValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<UmbReferenceByUnique[], ReadonlyArray<UmbUserGroupItemModel>>
{
	#repo = new UmbUserGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<UmbReferenceByUnique[]>,
	): Promise<UmbValueSummaryResolveResult<ReadonlyArray<UmbUserGroupItemModel>>> {
		const allUniques = [...new Set(values.flatMap((v) => v.map((r) => r.unique)))];

		if (allUniques.length === 0) {
			return { data: values.map(() => []) };
		}

		const { data, asObservable } = await this.#repo.requestItems(allUniques);
		const items = Array.isArray(data) ? data : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<UmbReferenceByUnique[]>,
		items: ReadonlyArray<UmbUserGroupItemModel>,
	): ReadonlyArray<ReadonlyArray<UmbUserGroupItemModel>> {
		const itemByUnique = new Map(items.map((g) => [g.unique, g]));
		return values.map((v) => v.map((r) => itemByUnique.get(r.unique)).filter(Boolean) as UmbUserGroupItemModel[]);
	}
}

// Named 'api' for ApiLoaderProperty convention
export { UmbUserGroupValueSummaryResolver as api };
