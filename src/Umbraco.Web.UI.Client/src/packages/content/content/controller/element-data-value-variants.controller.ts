import type { UmbElementWorkspaceDataManager } from '../manager/element-data-manager.js';
import type { UmbElementDetailModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { throttleTime } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbObjectWithVariantProperties } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPropertyValueFlatMapperController } from '@umbraco-cms/backoffice/property';

export class UmbElementDataValueVariantsController<ModelType extends UmbElementDetailModel> extends UmbControllerBase {
	#dataManager: UmbElementWorkspaceDataManager<ModelType>;

	#variants: UmbArrayState<UmbObjectWithVariantProperties> = new UmbArrayState<UmbObjectWithVariantProperties>(
		[],
		(v) => v.culture + ':' + v.segment,
	);
	/** An observable of the current variants that has values in the data. */
	public readonly variants = this.#variants.asObservable();

	constructor(host: UmbControllerHost, dataManager: UmbElementWorkspaceDataManager<ModelType>) {
		super(host);
		this.#dataManager = dataManager;

		this.observe(
			this.#dataManager.current.pipe(throttleTime(500, undefined, { leading: true, trailing: true })),
			(current) => {
				if (!current) return;
				this.#retrieveVariantsInData(current);
			},
			null,
		);
	}

	async #retrieveVariantsInData(current: ModelType) {
		// Get a flat map of values:
		const variants = await new UmbPropertyValueFlatMapperController(this).flatMapMany(current.values, (property) => {
			// Be aware that a value of `false` or `""` or `[]` is not captured here:
			if (property.value) {
				return { culture: property.culture, segment: property.segment };
			}
			return undefined;
		});

		const filteredVariants = variants.filter((v) => !!v);

		this.#variants.setValue(filteredVariants);
	}
}
