import type { UmbElementDetailModel } from '../types.js';
import type { UmbElementPropertyDataOwner } from './element-property-data-owner.interface.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import {
	UmbBasicState,
	UmbBooleanState,
	classEqualMemoization,
	createObservablePart,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

type UmbPropertyVariantIdMapType = Array<{ alias: string; variantId: UmbVariantId }>;

export abstract class UmbElementPropertyDatasetContext<
		ContentModel extends UmbElementDetailModel = UmbElementDetailModel,
		ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
		DataOwnerType extends UmbElementPropertyDataOwner<ContentModel, ContentTypeModel> = UmbElementPropertyDataOwner<
			ContentModel,
			ContentTypeModel
		>,
	>
	extends UmbContextBase<UmbPropertyDatasetContext>
	implements UmbPropertyDatasetContext
{
	protected readonly _dataOwner: DataOwnerType;
	#variantId: UmbVariantId;
	public getVariantId() {
		return this.#variantId;
	}

	abstract name: Observable<string | undefined>;
	abstract culture: Observable<string | null | undefined>;
	abstract segment: Observable<string | null | undefined>;

	#propertyVariantIdPromise?: Promise<never>;
	#propertyVariantIdPromiseResolver?: () => void;
	#propertyVariantIdMap = new UmbBasicState<UmbPropertyVariantIdMapType>([]);
	private readonly _propertyVariantIdMap = this.#propertyVariantIdMap.asObservable();

	protected _readOnly = new UmbBooleanState(false);
	public readOnly = this._readOnly.asObservable();

	getEntityType(): string {
		return this._dataOwner.getEntityType();
	}
	getUnique(): UmbEntityUnique | undefined {
		return this._dataOwner.getUnique();
	}
	abstract getName(): string | undefined;

	getReadOnly() {
		return this._readOnly.getValue();
	}

	constructor(host: UmbControllerHost, dataOwner: DataOwnerType, variantId?: UmbVariantId) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT);
		this._dataOwner = dataOwner;
		this.#variantId = variantId ?? UmbVariantId.CreateInvariant();

		this.#propertyVariantIdPromise = new Promise((resolve) => {
			this.#propertyVariantIdPromiseResolver = resolve as any;
		});

		this.observe(
			this._dataOwner.readOnlyGuard.isPermittedForVariant(this.#variantId),
			(isReadOnly) => {
				this._readOnly.setValue(isReadOnly);
			},
			null,
		);

		// TODO: Refactor this into a separate manager/controller of some sort? [NL]
		this.observe(
			this._dataOwner.structure.contentTypeProperties,
			(props: UmbPropertyTypeModel[]) => {
				const map = props.map((prop) => ({ alias: prop.alias, variantId: this.#createPropertyVariantId(prop) }));
				this.#propertyVariantIdMap.setValue(map);
				// Resolve promise, to let the once waiting on this know.
				if (this.#propertyVariantIdPromiseResolver) {
					this.#propertyVariantIdPromiseResolver();
					this.#propertyVariantIdPromiseResolver = undefined;
					this.#propertyVariantIdPromise = undefined;
				}
			},
			null,
		);
	}

	#createPropertyVariantId(property: UmbPropertyTypeModel) {
		return UmbVariantId.Create({
			culture: property.variesByCulture ? this.#variantId.culture : null,
			segment: property.variesBySegment ? this.#variantId.segment : null,
		});
	}

	#propertiesObservable?: Observable<ContentModel['values']>;
	// Should it be possible to get the properties as a list of property aliases?
	get properties(): Observable<ContentModel['values']> {
		if (!this.#propertiesObservable) {
			this.#propertiesObservable = mergeObservables(
				[this._propertyVariantIdMap, this._dataOwner.values],
				this.#mergeVariantIdsAndValues,
			);
		}

		return this.#propertiesObservable;
	}

	#mergeVariantIdsAndValues([props, values]: [UmbPropertyVariantIdMapType, ContentModel['values'] | undefined]) {
		const r: ContentModel['values'] = [];
		if (values) {
			for (const prop of props) {
				const f = values.find((v) => prop.alias === v.alias && prop.variantId.compare(v));
				if (f) {
					r.push(f);
				}
			}
		}
		return r as ContentModel['values'];
	}

	async getProperties(): Promise<ContentModel['values']> {
		await this.#propertyVariantIdPromise;
		return this.#mergeVariantIdsAndValues([
			this.#propertyVariantIdMap.getValue(),
			this._dataOwner.getValues(),
		]) as ContentModel['values'];
	}

	/**
	 * @function propertyVariantId
	 * @param {string} propertyAlias - The property alias to observe the variantId of.
	 * @returns {Promise<Observable<UmbVariantId | undefined> | undefined>} - The observable for this properties variantId.
	 * @description Get an Observable for the variant id of this property.
	 */
	async propertyVariantId(propertyAlias: string) {
		/*
		return (await this.#workspace.structure.propertyStructureByAlias(propertyAlias)).pipe(
			map((property) => (property ? this.#createPropertyVariantId(property) : undefined)),
		);
		*/
		return createObservablePart(
			this._propertyVariantIdMap,
			(x) => x.find((v) => v.alias === propertyAlias)?.variantId,
			classEqualMemoization,
		);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias  The alias of the property
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - An observable of the property value
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(
		propertyAlias: string,
	): Promise<Observable<ReturnType | undefined> | undefined> {
		await this._dataOwner.isLoaded();
		await this.#propertyVariantIdPromise;
		return mergeObservables(
			[await this.propertyVariantId(propertyAlias), this._dataOwner.values],
			([variantId, values]) => {
				return variantId
					? (values?.find((x) => x?.alias === propertyAlias && variantId.compare(x))?.value as ReturnType)
					: undefined;
			},
		);
	}

	// TODO: Refactor: Not used currently, but should investigate if we can implement this, to spare some energy.
	async propertyValueByAliasAndVariantId<ReturnType = unknown>(
		propertyAlias: string,
		propertyVariantId: UmbVariantId,
	): Promise<Observable<ReturnType | undefined> | undefined> {
		return this._dataOwner.propertyValueByAlias<ReturnType>(propertyAlias, propertyVariantId);
	}

	/**
	 * @function setPropertyValueByVariant
	 * @param {string} propertyAlias - The alias of the property
	 * @param {unknown} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @param {UmbVariantId} propertyVariantId - The variant id for the value to be set for.
	 * @returns {Promise<unknown>} - A promise that resolves once the value has been set.
	 * @description Get the value of this property.
	 */
	setPropertyValueByVariant(propertyAlias: string, value: unknown, propertyVariantId: UmbVariantId): Promise<void> {
		return this._dataOwner.setPropertyValue(propertyAlias, value, propertyVariantId);
	}

	/**
	 * @function setPropertyValue
	 * @param {string} propertyAlias - The alias for the value to be set
	 * @param {PromiseLike<unknown>} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue(propertyAlias: string, value: PromiseLike<unknown>) {
		this._dataOwner.initiatePropertyValueChange();
		await this.#propertyVariantIdPromise;
		const propVariantId = this.#propertyVariantIdMap.getValue().find((x) => x.alias === propertyAlias)?.variantId;
		if (propVariantId) {
			await this._dataOwner.setPropertyValue(propertyAlias, await value, propVariantId);
		}
		this._dataOwner.finishPropertyValueChange();
	}

	override destroy() {
		super.destroy();
		this.#propertyVariantIdMap?.destroy();
		(this.#propertyVariantIdMap as unknown) = undefined;
	}
}
