import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';

export interface UmbWorkspaceCollectionContextInterface<T extends UmbContentTypeModel>
	extends UmbWorkspaceContextInterface {
	contentTypeHasCollection: Observable<boolean>;
	getCollectionAlias(): string;
	structure: UmbContentTypeStructureManager<T>;
}
