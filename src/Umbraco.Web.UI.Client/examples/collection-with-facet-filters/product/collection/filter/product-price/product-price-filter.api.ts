import type { ExampleProductPriceFacetModel, ExampleProductPriceFilterValueModel } from './types.js';
import { UMB_FILTER_CONTEXT } from '@umbraco-cms/backoffice/filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface ExampleProductPriceFilterStateModel {
	range: ExampleProductPriceFacetModel;
	limits: { min?: number; max?: number };
}

export class ExampleProductPriceFilterApi extends UmbControllerBase {
	#state = new UmbObjectState<ExampleProductPriceFilterStateModel>({ range: { min: 0, max: 0 }, limits: {} });

	public readonly min = this.#state.asObservablePart((state) => state.range.min);
	public readonly max = this.#state.asObservablePart((state) => state.range.max);

	/**
	 * The selected range: each end is its active limit, or the facet's bound when that limit isn't set,
	 * kept within the facet's bounds so a limit outside them shows as the bound.
	 */
	public readonly currentMin = this.#state.asObservablePart(({ range, limits }) =>
		Math.min(Math.max(limits.min ?? range.min, range.min), range.max),
	);
	public readonly currentMax = this.#state.asObservablePart(({ range, limits }) =>
		Math.max(Math.min(limits.max ?? range.max, range.max), range.min),
	);

	#filterContext?: typeof UMB_FILTER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FILTER_CONTEXT, (context) => {
			this.#filterContext = context;

			this.observe(
				context?.facetedResult,
				(result) => {
					const range = result as ExampleProductPriceFacetModel | undefined;
					if (range && range.max > 0) {
						this.#state.update({ range });
					}
				},
				'exampleProductPriceFacetedResultObserver',
			);

			this.observe(
				context?.values,
				(entries) => {
					const amountOf = (key: string) =>
						(entries?.find((entry) => entry.unique === key)?.value as ExampleProductPriceFilterValueModel | undefined)
							?.amount;
					this.#state.update({ limits: { min: amountOf('min'), max: amountOf('max') } });
				},
				'exampleProductPriceFilterValuesObserver',
			);
		});
	}

	/**
	 * Sets the selected range. An end at the price facet's bound sets no limit, so it keeps following the facet.
	 * @param {number} min - The lower end of the selected range.
	 * @param {number} max - The upper end of the selected range.
	 */
	public setValue(min: number, max: number): void {
		const { range } = this.#state.getValue();
		const limits: Array<ExampleProductPriceFilterValueModel> = [];
		if (min > range.min) limits.push({ key: 'min', amount: min });
		if (max < range.max) limits.push({ key: 'max', amount: max });
		this.#filterContext?.setValues(limits.map((value) => ({ unique: value.key, value })));
	}

	public clear(): void {
		this.#filterContext?.clearAllValues();
	}
}

export { ExampleProductPriceFilterApi as api };
