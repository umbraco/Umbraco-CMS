import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';

export class UmbDocumentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | undefined, Array<string>>
{
	#repo = new UmbDocumentItemRepository(this);
	async resolveValues(values: ReadonlyArray<string | undefined>): Promise<UmbValueSummaryResolveResult<Array<string>>> {
		const allKeys = [...new Set(values.flatMap((v) => splitStringToArray(v)))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbDocumentItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbDocumentItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string | undefined>,
		items: ReadonlyArray<UmbDocumentItemModel>,
	): ReadonlyArray<Array<string>> {
		const nameByKey = new Map(items.map((item) => [item.unique, item.variants[0]?.name ?? '']));
		return values.map((v) =>
			splitStringToArray(v)
				.map((key) => nameByKey.get(key))
				.filter((n): n is string => !!n),
		);
	}
}

export { UmbDocumentPickerValueSummaryResolver as valueResolver };
