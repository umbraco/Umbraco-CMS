import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import type { UmbDocumentItemModel } from '../../../item/repository/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

/** Batch-resolves Multiple Document Picker value (array of document uniques) to their item models. */
export class UmbMultipleDocumentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string> | undefined, Array<UmbDocumentItemModel>>
{
	readonly #repo = new UmbDocumentItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<Array<string> | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbDocumentItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => v ?? []))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbDocumentItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (latest) => this.#map(values, latest as Array<UmbDocumentItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<Array<string> | undefined>,
		items: ReadonlyArray<UmbDocumentItemModel>,
	): ReadonlyArray<Array<UmbDocumentItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			(v ?? []).map((key) => itemByKey.get(key)).filter((item): item is UmbDocumentItemModel => !!item),
		);
	}
}
