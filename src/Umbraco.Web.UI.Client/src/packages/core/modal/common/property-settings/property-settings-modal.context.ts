import type { UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export const UMB_PROPERTY_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.PropertyType';

/**
 * This is a very simplified workspace context, just to serve one for the imitated property type workspace. (As its not a real workspace)
 */
export class UmbPropertyTypeWorkspaceContext
	extends UmbContextBase<UmbPropertyTypeWorkspaceContext>
	implements UmbWorkspaceContextInterface
{
	constructor(host: UmbControllerHostElement) {
		// TODO: We don't need a repo here, so maybe we should not require this of the UmbEditableWorkspaceContextBase
		super(host, UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT);
	}

	get workspaceAlias() {
		return UMB_PROPERTY_TYPE_WORKSPACE_ALIAS;
	}

	getEntityId() {
		return undefined;
	}

	getEntityType() {
		return 'property-type';
	}
}

export default UmbPropertyTypeWorkspaceContext;

export const UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbPropertyTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPropertyTypeWorkspaceContext => context.getEntityType() === 'property-type',
);
