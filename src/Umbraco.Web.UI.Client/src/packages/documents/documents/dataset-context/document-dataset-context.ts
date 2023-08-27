import { UmbDocumentWorkspaceContext } from "../workspace/index.js";
import { DocumentVariantResponseModel, PropertyTypeModelBaseModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbBaseController, UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { map } from "@umbraco-cms/backoffice/external/rxjs";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UmbVariantId } from "@umbraco-cms/backoffice/variant";
import { UMB_DATASET_CONTEXT, UmbVariantDatasetContext } from "@umbraco-cms/backoffice/workspace";

export class UmbDocumentDatasetContext extends UmbBaseController implements UmbVariantDatasetContext {

	#workspace: UmbDocumentWorkspaceContext;
	#variantId: UmbVariantId;
	public getVariantId() {
		return this.#variantId;
	}

	#currentVariant = new UmbObjectState<DocumentVariantResponseModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.asObservablePart((x) => x?.name);
	culture = this.#currentVariant.asObservablePart((x) => x?.culture);
	segment = this.#currentVariant.asObservablePart((x) => x?.segment);

	// TODO: Refactor: Make a properties observable. (with such I think i mean a property value object array.. array with object with properties, alias, value, culture and segment)



	getType(): string {
		return this.#workspace.getEntityType();
	}
	getUnique(): string | undefined {
		return this.#workspace.getEntityId();
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



	constructor(host: UmbControllerHost, workspace: UmbDocumentWorkspaceContext, variantId: UmbVariantId) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, 'dataSetContext');
		this.#workspace = workspace;
		this.#variantId = variantId;

		this.observe(
			this.#workspace.variantById(this.#variantId),
			async (variantInfo) => {
				if (!variantInfo) return;
				this.#currentVariant.next(variantInfo);
			},
			'_observeActiveVariant'
		);

		// TODO: Refactor: use the document dataset context token.
		this.provideContext(UMB_DATASET_CONTEXT, this);
	}


	#createPropertyVariantId(property:PropertyTypeModelBaseModel) {
		return UmbVariantId.Create({
			culture: property.variesByCulture ? this.#variantId.culture : null,
			segment: property.variesBySegment ? this.#variantId.segment : null,
		});
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async propertyVariantId(propertyAlias: string) {
		return (await this.#workspace.structure.propertyStructureByAlias(propertyAlias)).pipe(map((property) => property ? this.#createPropertyVariantId(property) : undefined));
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {

		return (await this.#workspace.structure.propertyStructureByAlias(propertyAlias)).pipe(map((property) => property?.alias ? this.#workspace.getPropertyValue<ReturnType>(property.alias, this.#createPropertyVariantId(property)) : undefined));

		/*
		// This is not reacting to if the property variant settings changes while running.
		const property = await this.#workspace.structure.getPropertyStructureByAlias(propertyAlias);
		if(property) {
			const variantId = this.#createPropertyVariantId(property);
			if(property.alias) {
				return this.#workspace.propertyValueByAlias<ReturnType>(property.alias, variantId);
			}
		}
		return undefined;
		*/
	}

	// TODO: Refactor:
	// Not used currently:
	async propertyValueByAliasAndCulture<ReturnType = unknown>(propertyAlias: string, propertyVariantId: UmbVariantId) {
		return this.#workspace.propertyValueByAlias<ReturnType>(propertyAlias, propertyVariantId);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		// This is not reacting to if the property variant settings changes while running.
		const property = await this.#workspace.structure.getPropertyStructureByAlias(propertyAlias);
		if(property) {
			const variantId = this.#createPropertyVariantId(property);

			// This is not reacting to if the property variant settings changes while running.
			this.#workspace.setPropertyValue(propertyAlias, value, variantId);
		}
	}
}
