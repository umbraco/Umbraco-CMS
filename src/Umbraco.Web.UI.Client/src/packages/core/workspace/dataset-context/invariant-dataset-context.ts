import { DocumentVariantResponseModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbBaseController, UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UMB_DATASET_CONTEXT, UmbDatasetContext, UmbInvariantableWorkspaceContextInterface } from "@umbraco-cms/backoffice/workspace";

export class UmbInvariantDatasetContext extends UmbBaseController implements UmbDatasetContext {

	#workspace: UmbInvariantableWorkspaceContextInterface;

	#currentVariant = new UmbObjectState<DocumentVariantResponseModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.asObservablePart((x) => x?.name);
	culture = this.#currentVariant.asObservablePart((x) => x?.culture);
	segment = this.#currentVariant.asObservablePart((x) => x?.segment);

	// default data:


	getType(): string {
		return this.#workspace.getEntityType();
	}
	getUnique(): string | undefined {
		return this.#workspace.getEntityId();
	}
	getName() {
		return this.#workspace.getName();
	}
	setName(name: string) {
		this.#workspace.setName(name);
	}



	constructor(host: UmbControllerHost, workspace: UmbInvariantableWorkspaceContextInterface) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, 'dataSetContext');
		this.#workspace = workspace;

		// TODO: Refactor: use the document dataset context token.
		this.provideContext(UMB_DATASET_CONTEXT, this);
	}



	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#workspace.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this.#workspace.setPropertyValue(propertyAlias, value);
	}
}
