import { UmbMemberTypeItemRepository } from '@umbraco-cms/backoffice/member-type';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbDisplayBatchResolveResult } from '@umbraco-cms/backoffice/display-batch-resolver';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

/**
 * Resolves the active values of the member type collection filter, member type uniques, into member type items.
 */
export class UmbMemberTypeFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<string, UmbItemModel>
{
	#itemRepository = new UmbMemberTypeItemRepository(this);

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

export { UmbMemberTypeFacetFilterValueResolver as valueResolver };
