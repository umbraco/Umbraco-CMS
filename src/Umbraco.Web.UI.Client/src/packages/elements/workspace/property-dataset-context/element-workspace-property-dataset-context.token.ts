import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementWorkspacePropertyDatasetContext } from './element-workspace-property-dataset-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';

const IsElementPropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbElementWorkspacePropertyDatasetContext => context.getEntityType() === UMB_ELEMENT_ENTITY_TYPE;

export const UMB_ELEMENT_WORKSPACE_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbElementWorkspacePropertyDatasetContext
>(UMB_PROPERTY_DATASET_CONTEXT.toString(), undefined, IsElementPropertyDatasetContext);
