import type { UmbContentCollectionManager } from './manager/content-collection-manager.controller.js';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbContentCollectionWorkspaceContext<T extends UmbContentTypeModel> extends UmbEntityWorkspaceContext {
	collection: UmbContentCollectionManager;
	structure: UmbContentTypeStructureManager<T>;
}
