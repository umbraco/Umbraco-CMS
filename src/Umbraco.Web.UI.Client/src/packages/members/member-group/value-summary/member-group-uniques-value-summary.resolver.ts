import { UmbMemberGroupItemRepository } from '../repository/item/member-group-item.repository.js';
import type { UmbMemberGroupItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbMemberGroupUniquesValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string>, Array<UmbMemberGroupItemModel>>
{
	#repo = new UmbMemberGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<Array<string>>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMemberGroupItemModel>>> {
		const allUniques = [...new Set(values.flat())];
		if (!allUniques.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allUniques);
		const items = Array.isArray(data) ? data : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbMemberGroupItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<Array<string>>,
		items: ReadonlyArray<UmbMemberGroupItemModel>,
	): ReadonlyArray<Array<UmbMemberGroupItemModel>> {
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			v.map((unique) => itemByUnique.get(unique)).filter((item): item is UmbMemberGroupItemModel => !!item),
		);
	}
}
