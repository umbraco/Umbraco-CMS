import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbMemberGroupItemRepository } from '../../../repository/item/index.js';
import type { UmbMemberGroupItemModel } from '../../../repository/item/types.js';

export class UmbMemberGroupPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | undefined, Array<UmbMemberGroupItemModel>>
{
	#repo = new UmbMemberGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMemberGroupItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => splitStringToArray(v)))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbMemberGroupItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbMemberGroupItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string | undefined>,
		items: ReadonlyArray<UmbMemberGroupItemModel>,
	): ReadonlyArray<Array<UmbMemberGroupItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			splitStringToArray(v)
				.map((key) => itemByKey.get(key))
				.filter((item): item is UmbMemberGroupItemModel => !!item),
		);
	}
}

export { UmbMemberGroupPickerValueSummaryResolver as valueResolver };
