import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbContentCollectionWorkspaceContext<T extends UmbContentTypeModel> extends UmbEntityWorkspaceContext {
	contentTypeHasCollection: Observable<boolean>;
	getCollectionAlias(): string;
	structure: UmbContentTypeStructureManager<T>;
}
