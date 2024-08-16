import { UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT } from './block-element-property-dataset.context-token.js';
import type { UmbBlockElementManager } from './block-element-manager.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbBlockElementPropertyDatasetContext extends UmbControllerBase implements UmbPropertyDatasetContext {
	#elementManager: UmbBlockElementManager;

	// default data:

	getVariantId() {
		return UmbVariantId.CreateInvariant();
	}
	getEntityType() {
		return this.#elementManager.getEntityType();
	}
	getUnique() {
		return this.#elementManager.getUnique();
	}

	getName(): string | undefined {
		return 'TODO: get label';
	}
	readonly name: Observable<string | undefined> = 'TODO: get label observable' as any;

	constructor(host: UmbControllerHost, elementManager: UmbBlockElementManager) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT.toString());
		this.#elementManager = elementManager;

		this.provideContext(UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT, this);
	}

	propertyVariantId?(propertyAlias: string): Promise<Observable<UmbVariantId | undefined>> {
		return this.#elementManager.propertyVariantId(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * @param propertyAlias
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return await this.#elementManager.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * @param propertyAlias
	 * @param value
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this.#elementManager.setPropertyValue(propertyAlias, value);
	}
}
