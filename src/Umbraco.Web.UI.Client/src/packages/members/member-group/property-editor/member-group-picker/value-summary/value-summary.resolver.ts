import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbMemberGroupItemRepository } from '@umbraco-cms/backoffice/member-group';
import type { UmbMemberGroupItemModel } from '@umbraco-cms/backoffice/member-group';

export class UmbMemberGroupPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | undefined, Array<string>>
{
	#repo = new UmbMemberGroupItemRepository(this);
	async resolveValues(values: ReadonlyArray<string | undefined>): Promise<UmbValueSummaryResolveResult<Array<string>>> {
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
	): ReadonlyArray<Array<string>> {
		const nameByKey = new Map(items.map((item) => [item.unique, item.name]));
		return values.map((v) =>
			splitStringToArray(v)
				.map((key) => nameByKey.get(key))
				.filter((n): n is string => !!n),
		);
	}
}

export { UmbMemberGroupPickerValueSummaryResolver as valueResolver };
