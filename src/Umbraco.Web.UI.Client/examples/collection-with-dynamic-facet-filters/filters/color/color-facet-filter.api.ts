import { UMB_FACET_FILTER_CONTEXT } from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface ExampleColorFacetOptionModel {
	unique: string;
	name: string;
	hex: string;
	count: number;
}

export class ExampleColorFacetFilterApi extends UmbControllerBase {
	#options = new UmbArrayState<ExampleColorFacetOptionModel>([], (x) => x.unique);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#facetFilterContext?: typeof UMB_FACET_FILTER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FACET_FILTER_CONTEXT, (context) => {
			this.#facetFilterContext = context;
			this.observe(
				context?.facetedResult,
				(result) => this.#options.setValue((result as Array<ExampleColorFacetOptionModel> | undefined) ?? []),
				'exampleColorFacetedResultObserver',
			);
			this.observe(
				context?.values,
				(entries) => this.#value.setValue(entries?.map((entry) => entry.value as string) ?? []),
				'exampleColorFilterValuesObserver',
			);
		});
	}

	public setValue(values: Array<string>): void {
		if (values.length === 0) {
			this.#facetFilterContext?.clearAllValues();
		} else {
			this.#facetFilterContext?.setValues(values.map((value) => ({ unique: value, value })));
		}
	}
}

export { ExampleColorFacetFilterApi as api };
