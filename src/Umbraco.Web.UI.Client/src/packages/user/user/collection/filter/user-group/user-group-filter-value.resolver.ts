import { UmbUserGroupItemRepository, type UmbUserGroupItemModel } from '@umbraco-cms/backoffice/user-group';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import type { UmbDisplayBatchResolveResult } from '@umbraco-cms/backoffice/display-batch-resolver';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

/**
 * Resolves the active values of the user group collection filter, user group uniques, into user group items.
 */
export class UmbUserGroupFilterValueResolver
	extends UmbControllerBase
	implements UmbFilterValueResolver<string, UmbUserGroupItemModel>
{
	#itemRepository = new UmbUserGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string>,
	): Promise<UmbDisplayBatchResolveResult<UmbUserGroupItemModel | undefined>> {
		const { data, asObservable } = await this.#itemRepository.requestItems([...new Set(values)]);

		return {
			data: this.#map(values, data ?? []),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string>,
		items: ReadonlyArray<UmbUserGroupItemModel>,
	): ReadonlyArray<UmbUserGroupItemModel | undefined> {
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));
		return values.map((value) => itemByUnique.get(value));
	}
}

export { UmbUserGroupFilterValueResolver as valueResolver };
