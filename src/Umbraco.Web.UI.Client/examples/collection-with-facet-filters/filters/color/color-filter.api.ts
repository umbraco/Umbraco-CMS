import { colors } from './colors.js';
import { UMB_FILTER_CONTEXT } from '@umbraco-cms/backoffice/filter';
import type { UmbFacetModel } from '@umbraco-cms/backoffice/filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface ExampleColorFacetOptionModel extends UmbFacetModel {
	name: string;
	hex: string;
}

export class ExampleColorFilterApi extends UmbControllerBase {
	#options = new UmbArrayState<ExampleColorFacetOptionModel>([], (x) => x.unique);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#filterContext?: typeof UMB_FILTER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FILTER_CONTEXT, (context) => {
			this.#filterContext = context;
			this.observe(
				context?.facetedResult,
				(result) => this.#options.setValue(((result as Array<UmbFacetModel> | undefined) ?? []).map(this.#toOption)),
				'exampleColorFacetedResultObserver',
			);
			this.observe(
				context?.values,
				(entries) => this.#value.setValue(entries?.map((entry) => entry.value as string) ?? []),
				'exampleColorFilterValuesObserver',
			);
		});
	}

	#toOption = (facet: UmbFacetModel): ExampleColorFacetOptionModel => {
		const color = colors.find((item) => item.id === facet.unique);
		return { ...facet, name: color?.name ?? facet.unique, hex: color?.hex ?? facet.unique };
	};

	public setValue(values: Array<string>): void {
		if (values.length === 0) {
			this.#filterContext?.clearAllValues();
		} else {
			this.#filterContext?.setValues(values.map((value) => ({ unique: value, value })));
		}
	}
}

export { ExampleColorFilterApi as api };
