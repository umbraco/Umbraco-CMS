import { UmbEntityWorkspaceContextInterface } from './workspace-entity-context.interface';
import { PropertyTypeResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

type BaseEntityType = {
	key?: string;
	alias?: string;
	name?: string;
	icon?: string;
	properties?: Array<PropertyTypeResponseModelBaseModel>;
};

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface<BaseEntityType>>(
	'UmbEntityWorkspaceContext'
);
