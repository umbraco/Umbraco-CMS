import { DocumentVariantResponseModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbBaseController, UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UmbVariantId } from "@umbraco-cms/backoffice/variant";
import { UMB_VARIANT_CONTEXT, UmbVariantContext, UmbInvariantableWorkspaceContextInterface } from "@umbraco-cms/backoffice/workspace";

export class UmbInvariantWorkspaceVariantContext<WorkspaceType extends UmbInvariantableWorkspaceContextInterface= UmbInvariantableWorkspaceContextInterface> extends UmbBaseController implements UmbVariantContext {

	protected _workspace: WorkspaceType;

	#currentVariant = new UmbObjectState<DocumentVariantResponseModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.asObservablePart((x) => x?.name);
	culture = this.#currentVariant.asObservablePart((x) => x?.culture);
	segment = this.#currentVariant.asObservablePart((x) => x?.segment);

	// default data:

	getVariantId() {
		return UmbVariantId.CreateInvariant();
	}
	getType() {
		return this._workspace.getEntityType();
	}
	getUnique() {
		return this._workspace.getEntityId();
	}
	getName() {
		return this._workspace.getName();
	}
	setName(name: string) {
		this._workspace.setName(name);
	}



	constructor(host: UmbControllerHost, workspace: WorkspaceType) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, 'variantContext');
		this._workspace = workspace;

		this.provideContext(UMB_VARIANT_CONTEXT, this);
	}



	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this._workspace.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 * Ideally do not use these methods, its better to communicate directly with the workspace, but if you do not know the property variant id, then this will figure it out for you. So good for externals to set or get values of a property.
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this._workspace.setPropertyValue(propertyAlias, value);
	}
}
