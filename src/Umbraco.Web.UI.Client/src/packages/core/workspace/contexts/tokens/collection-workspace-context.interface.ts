import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';

export interface UmbCollectionWorkspaceContext<T extends UmbContentTypeModel> extends UmbEntityWorkspaceContext {
	contentTypeHasCollection: Observable<boolean>;
	getCollectionAlias(): string;
	structure: UmbContentTypeStructureManager<T>;
}
