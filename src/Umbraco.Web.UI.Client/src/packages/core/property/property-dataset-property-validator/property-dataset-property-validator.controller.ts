import { UMB_PROPERTY_DATASET_CONTEXT } from '../property-dataset/property-dataset-context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbValueValidator, type UmbValueValidatorArgs } from '@umbraco-cms/backoffice/validation';

export interface UmbPropertyDatasetPropertyValidatorArgs<ValueType = unknown> extends UmbValueValidatorArgs<ValueType> {
	propertyAlias: string;
}

// The Example Workspace Context Controller:
export class UmbPropertyDatasetPropertyValidator<ValueType = unknown> extends UmbValueValidator<ValueType> {
	//
	#propertyAlias: string;

	constructor(host: UmbControllerHost, args: UmbPropertyDatasetPropertyValidatorArgs<ValueType>) {
		super(host, args);
		this.#propertyAlias = args.propertyAlias;

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(
				await context?.propertyValueByAlias<ValueType>(this.#propertyAlias),
				(value) => {
					this.value = value;
				},
				'observeDatasetValue',
			);
		});
	}
}
