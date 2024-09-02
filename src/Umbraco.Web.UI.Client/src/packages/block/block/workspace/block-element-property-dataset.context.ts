import { UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT } from './block-element-property-dataset.context-token.js';
import type { UmbBlockElementManager } from './block-element-manager.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

export class UmbBlockElementPropertyDatasetContext extends UmbControllerBase implements UmbPropertyDatasetContext {
	#elementManager: UmbBlockElementManager;
	#variantId: UmbVariantId;
	public getVariantId() {
		return this.#variantId;
	}

	#currentVariantCultureIsReadOnly = new UmbBooleanState(false);
	public currentVariantCultureIsReadOnly = this.#currentVariantCultureIsReadOnly.asObservable();

	// default data:

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

	constructor(host: UmbControllerHost, elementManager: UmbBlockElementManager, variantId?: UmbVariantId) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT.toString());
		this.#elementManager = elementManager;
		this.#variantId = variantId ?? UmbVariantId.CreateInvariant();

		/*
		this.observe(
			this.#elementManager.readOnlyState.states,
			(states) => {
				const isReadOnly = states.some(
					(state) => state.unique.startsWith('UMB_CULTURE_') && state.variantId.equal(this.#variantId),
				);

				this.#currentVariantCultureIsReadOnly.setValue(isReadOnly);
			},
			'umbObserveReadOnlyStates',
		);
		*/

		this.provideContext(UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT, this);
	}

	#createPropertyVariantId(property: UmbPropertyTypeModel) {
		return UmbVariantId.Create({
			culture: property.variesByCulture ? this.#variantId.culture : null,
			segment: property.variesBySegment ? this.#variantId.segment : null,
		});
	}

	propertyVariantId?(propertyAlias: string): Promise<Observable<UmbVariantId | undefined>> {
		return this.#elementManager.propertyVariantId(propertyAlias);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return await this.#elementManager.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias
	 * @param {unknown} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue(alias: string, value: PromiseLike<unknown>) {
		this.#elementManager.initiatePropertyValueChange();
		// This is not reacting to if the property variant settings changes while running.
		const property = await this.#elementManager.structure.getPropertyStructureByAlias(alias);
		if (property) {
			const variantId = this.#createPropertyVariantId(property);

			// This is not reacting to if the property variant settings changes while running.
			this.#elementManager.setPropertyValue(alias, await value, variantId);
		}
		this.#elementManager.finishPropertyValueChange();
	}

	/**
	 * Gets the read-only state of the current variant culture.
	 * @returns {*}  {boolean}
	 * @memberof UmbBlockGridInlinePropertyDatasetContext
	 */
	getCurrentVariantCultureIsReadOnly(): boolean {
		return this.#currentVariantCultureIsReadOnly.getValue();
	}
}
