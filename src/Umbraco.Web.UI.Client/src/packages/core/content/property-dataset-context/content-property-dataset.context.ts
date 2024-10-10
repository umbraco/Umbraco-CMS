import type { UmbContentWorkspaceContext } from '../workspace/index.js';
import type { UmbContentDetailModel } from '../types.js';
import type { UmbNameablePropertyDatasetContext, UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { type Observable, map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	UmbBasicState,
	UmbBooleanState,
	UmbObjectState,
	classEqualMemoization,
	createObservablePart,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceUniqueType } from '@umbraco-cms/backoffice/workspace';

type UmbPropertyVariantIdMapType = Array<{ alias: string; variantId: UmbVariantId }>;

export class UmbContentPropertyDatasetContext<
		ContentModel extends UmbContentDetailModel = UmbContentDetailModel,
		ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
		VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
	>
	extends UmbContextBase<UmbPropertyDatasetContext>
	implements UmbPropertyDatasetContext, UmbNameablePropertyDatasetContext
{
	#workspace: UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>;
	#variantId: UmbVariantId;
	public getVariantId() {
		return this.#variantId;
	}

	#currentVariant = new UmbObjectState<UmbEntityVariantModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.asObservablePart((x) => x?.name);
	culture = this.#currentVariant.asObservablePart((x) => x?.culture);
	segment = this.#currentVariant.asObservablePart((x) => x?.segment);

	// eslint-disable-next-line no-unused-private-class-members
	#propertyVariantIdPromise?: Promise<never>;
	#propertyVariantIdPromiseResolver?: () => void;
	#propertyVariantIdMap = new UmbBasicState<UmbPropertyVariantIdMapType>([]);
	private readonly _propertyVariantIdMap = this.#propertyVariantIdMap.asObservable();

	#readOnly = new UmbBooleanState(false);
	public readOnly = this.#readOnly.asObservable();

	getEntityType(): string {
		return this.#workspace.getEntityType();
	}
	getUnique(): UmbWorkspaceUniqueType | undefined {
		return this.#workspace.getUnique();
	}
	getName(): string | undefined {
		return this.#workspace.getName(this.#variantId);
	}
	setName(name: string) {
		this.#workspace.setName(name, this.#variantId);
	}
	getVariantInfo() {
		return this.#workspace.getVariant(this.#variantId);
	}

	getReadOnly() {
		return this.#readOnly.getValue();
	}

	constructor(
		host: UmbControllerHost,
		workspace: UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>,
		variantId?: UmbVariantId,
	) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT);
		this.#workspace = workspace;
		this.#variantId = variantId ?? UmbVariantId.CreateInvariant();

		this.observe(
			this.#workspace.variantById(this.#variantId),
			async (variantInfo) => {
				if (!variantInfo) return;
				this.#currentVariant.setValue(variantInfo);
			},
			'_observeActiveVariant',
		);

		this.observe(
			this.#workspace.readOnlyState.states,
			(states) => {
				const isReadOnly = states.some((state) => state.variantId.equal(this.#variantId));
				this.#readOnly.setValue(isReadOnly);
			},
			'umbObserveReadOnlyStates',
		);

		// TODO: Refactor this into a separate manager/controller of some sort? [NL]
		this.observe(this.#workspace.structure.contentTypeProperties, (props: UmbPropertyTypeModel[]) => {
			this.#propertyVariantIdPromise ??= new Promise((resolve) => {
				this.#propertyVariantIdPromiseResolver = resolve as any;
			});
			const map = props.map((prop) => ({ alias: prop.alias, variantId: this.#createPropertyVariantId(prop) }));
			this.#propertyVariantIdMap.setValue(map);
			// Resolve promise, to let the once waiting on this know.
			if (this.#propertyVariantIdPromiseResolver) {
				this.#propertyVariantIdPromise = undefined;
				this.#propertyVariantIdPromiseResolver();
				this.#propertyVariantIdPromiseResolver = undefined;
			}
		});
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
				[this._propertyVariantIdMap, this.#workspace.values],
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
		return r;
	}

	async getProperties(): Promise<ContentModel['values']> {
		await this.#propertyVariantIdPromise;
		return this.#mergeVariantIdsAndValues([this.#propertyVariantIdMap.getValue(), this.#workspace.getValues()]);
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
		await this.#workspace.isLoaded();
		await this.#propertyVariantIdPromise;
		const propVariantId = this.#propertyVariantIdMap.getValue().find((x) => x.alias === propertyAlias);
		if (propVariantId) {
			return this.#workspace.propertyValueByAlias<ReturnType>(propertyAlias, propVariantId.variantId);
		}
		return;
	}

	// TODO: Refactor: Not used currently, but should investigate if we can implement this, to spare some energy.
	async propertyValueByAliasAndVariantId<ReturnType = unknown>(
		propertyAlias: string,
		propertyVariantId: UmbVariantId,
	): Promise<Observable<ReturnType | undefined> | undefined> {
		return this.#workspace.propertyValueByAlias<ReturnType>(propertyAlias, propertyVariantId);
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
		return this.#workspace.setPropertyValue(propertyAlias, value, propertyVariantId);
	}

	/**
	 * @function setPropertyValue
	 * @param {string} propertyAlias - The alias for the value to be set
	 * @param {PromiseLike<unknown>} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue(propertyAlias: string, value: PromiseLike<unknown>) {
		this.#workspace.initiatePropertyValueChange();
		await this.#propertyVariantIdPromise;
		const propVariantId = this.#propertyVariantIdMap.getValue().find((x) => x.alias === propertyAlias);
		if (propVariantId) {
			return this.#workspace.setPropertyValue(propertyAlias, await value, propVariantId.variantId);
		}
		this.#workspace.finishPropertyValueChange();
	}
}
