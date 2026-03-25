import { UMB_FACET_FILTER_CONTEXT } from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface ExampleRangeResult {
	min: number;
	max: number;
}

export class ExampleRangeFacetFilterApi extends UmbControllerBase {
	#range = new UmbObjectState<ExampleRangeResult>({ min: 0, max: 0 });
	public readonly min = this.#range.asObservablePart((r) => r.min);
	public readonly max = this.#range.asObservablePart((r) => r.max);

	#current = new UmbObjectState<ExampleRangeResult>({ min: 0, max: 0 });
	public readonly currentMin = this.#current.asObservablePart((r) => r.min);
	public readonly currentMax = this.#current.asObservablePart((r) => r.max);

	#facetFilterContext?: typeof UMB_FACET_FILTER_CONTEXT.TYPE;
	#initialized = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FACET_FILTER_CONTEXT, (context) => {
			this.#facetFilterContext = context;
			this.#observeFacetedResult();
			this.#observeFilterValues();
		});
	}

	#observeFacetedResult() {
		if (!this.#facetFilterContext) return;
		this.observe(
			this.#facetFilterContext.facetedResult,
			(result) => {
				const range = result as ExampleRangeResult | undefined;
				if (range && range.max > 0) {
					this.#range.setValue(range);
					if (!this.#initialized) {
						this.#current.setValue(range);
						this.#initialized = true;
					}
				}
			},
			'umbRangeFacetedResultObserver',
		);
	}

	#observeFilterValues() {
		if (!this.#facetFilterContext) return;
		this.observe(
			this.#facetFilterContext.values,
			(entries) => {
				if (entries && entries.length > 0) {
					const minEntry = entries.find((e) => e.unique === 'min');
					const maxEntry = entries.find((e) => e.unique === 'max');
					if (minEntry && maxEntry) {
						this.#current.setValue({
							min: minEntry.value as number,
							max: maxEntry.value as number,
						});
					}
				}
			},
			'umbRangeFilterValuesObserver',
		);
	}

	public setValue(min: number, max: number): void {
		this.#current.setValue({ min, max });
		this.#facetFilterContext?.setValues([
			{ unique: 'min', value: min },
			{ unique: 'max', value: max },
		]);
	}

	public clear(): void {
		const range = this.#range.getValue();
		this.#current.setValue(range);
		this.#facetFilterContext?.clearAllValues();
	}
}

export { ExampleRangeFacetFilterApi as api };
