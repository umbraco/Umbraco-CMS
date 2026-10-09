import { UmbMemberGroupItemRepository } from '../repository/item/member-group-item.repository.js';
import type { UmbMemberGroupItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

export class UmbMemberGroupsValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | Array<string> | undefined, Array<UmbMemberGroupItemModel>>
{
	#repo = new UmbMemberGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string | Array<string> | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMemberGroupItemModel>>> {
		const allUniques = [...new Set(values.flatMap((v) => this.#toUniques(v)))];
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

	#toUniques(value: string | Array<string> | undefined): Array<string> {
		return Array.isArray(value) ? value : splitStringToArray(value);
	}

	#map(
		values: ReadonlyArray<string | Array<string> | undefined>,
		items: ReadonlyArray<UmbMemberGroupItemModel>,
	): ReadonlyArray<Array<UmbMemberGroupItemModel>> {
		const itemByUnique = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			this.#toUniques(v)
				.map((unique) => itemByUnique.get(unique))
				.filter((item): item is UmbMemberGroupItemModel => !!item),
		);
	}
}
