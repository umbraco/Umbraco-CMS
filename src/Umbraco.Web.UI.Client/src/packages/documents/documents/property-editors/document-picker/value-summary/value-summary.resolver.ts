import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import type { UmbDocumentItemModel } from '../../../item/repository/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

/**
 * The value of a document picker property: a comma-separated string of document uniques for a picker holding one
 * document, or an array of document uniques for a picker holding several.
 */
export type UmbDocumentPickerValueSummaryValue = string | Array<string> | undefined;

/**
 * Batch-resolves document picker values to the picked documents' item models, whichever shape the value takes.
 */
export class UmbDocumentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<UmbDocumentPickerValueSummaryValue, Array<UmbDocumentItemModel>>
{
	readonly #repo = new UmbDocumentItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<UmbDocumentPickerValueSummaryValue>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbDocumentItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => this.#toKeys(v)))];
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

	#toKeys(value: UmbDocumentPickerValueSummaryValue): Array<string> {
		return Array.isArray(value) ? value : splitStringToArray(value);
	}

	#map(
		values: ReadonlyArray<UmbDocumentPickerValueSummaryValue>,
		items: ReadonlyArray<UmbDocumentItemModel>,
	): ReadonlyArray<Array<UmbDocumentItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			this.#toKeys(v)
				.map((key) => itemByKey.get(key))
				.filter((item): item is UmbDocumentItemModel => !!item),
		);
	}
}
