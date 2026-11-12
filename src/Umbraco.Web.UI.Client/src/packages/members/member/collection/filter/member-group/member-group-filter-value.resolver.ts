import { UmbMemberGroupItemRepository } from '@umbraco-cms/backoffice/member-group';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import type { UmbDisplayBatchResolveResult } from '@umbraco-cms/backoffice/display-batch-resolver';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

/**
 * Resolves the active values of the member group collection filter, member group uniques, into member group items.
 */
export class UmbMemberGroupFilterValueResolver
	extends UmbControllerBase
	implements UmbFilterValueResolver<string, UmbItemModel>
{
	#itemRepository = new UmbMemberGroupItemRepository(this);

	async resolveValues(values: ReadonlyArray<string>): Promise<UmbDisplayBatchResolveResult<UmbItemModel | undefined>> {
		const { data, asObservable } = await this.#itemRepository.requestItems([...new Set(values)]);

		return {
			data: this.#map(values, data ?? []),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(values: ReadonlyArray<string>, items: ReadonlyArray<UmbItemModel>): ReadonlyArray<UmbItemModel | undefined> {
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));
		return values.map((value) => itemByUnique.get(value));
	}
}

export { UmbMemberGroupFilterValueResolver as valueResolver };
