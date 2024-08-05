import { UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT } from './block-element-property-dataset.context-token.js';
import type { UmbBlockElementManager } from './block-element-manager.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockElementPropertyDatasetContext extends UmbControllerBase implements UmbPropertyDatasetContext {
	#elementManager: UmbBlockElementManager;

	#currentVariantCultureIsReadOnly = new UmbBooleanState(false);
	public currentVariantCultureIsReadOnly = this.#currentVariantCultureIsReadOnly.asObservable();

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

	/**
	 * Gets the value of a property.
	 * @template ReturnType
	 * @param {string} propertyAlias
	 * @return {*}
	 * @memberof UmbBlockElementPropertyDatasetContext
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return await this.#elementManager.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * Sets the value of a property.
	 * @param {string} propertyAlias
	 * @param {unknown} value
	 * @return {*}
	 * @memberof UmbBlockElementPropertyDatasetContext
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this.#elementManager.setPropertyValue(propertyAlias, value);
	}

	/**
	 * Gets the read-only state of the current variant culture.
	 * @return {*}  {boolean}
	 * @memberof UmbBlockGridInlinePropertyDatasetContext
	 */
	getCurrentVariantCultureIsReadOnly(): boolean {
		return this.#currentVariantCultureIsReadOnly.getValue();
	}
}
