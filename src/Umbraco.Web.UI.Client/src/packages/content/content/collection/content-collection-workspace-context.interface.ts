import type { UmbContentCollectionManager } from './manager/content-collection-manager.controller.js';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

/**
 * @deprecated Deprecated since v18. Consume `UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT` for the collection
 * configuration instead, which is answered by any host rather than only a workspace. Scheduled for removal in
 * Umbraco 20.
 */
export interface UmbContentCollectionWorkspaceContext<T extends UmbContentTypeModel> extends UmbEntityWorkspaceContext {
	collection: UmbContentCollectionManager;
	structure: UmbContentTypeStructureManager<T>;
}
