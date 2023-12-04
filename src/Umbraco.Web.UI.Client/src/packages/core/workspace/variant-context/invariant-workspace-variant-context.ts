import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UMB_VARIANT_CONTEXT,
	UmbVariantContext,
	UmbInvariantableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';

export class UmbInvariantWorkspaceVariantContext<
		WorkspaceType extends UmbInvariantableWorkspaceContextInterface = UmbInvariantableWorkspaceContextInterface,
	>
	extends UmbBaseController
	implements UmbVariantContext
{
	protected _workspace: WorkspaceType;

	name;

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

		this.name = this._workspace.name;

		this.provideContext(UMB_VARIANT_CONTEXT, this);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return await this._workspace.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this._workspace.setPropertyValue(propertyAlias, value);
	}
}
