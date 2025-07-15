import type { UmbContentTypeModel, UmbPropertyTypeModel } from '../types.js';
import type { UmbContentTypeStructureManager } from '../structure/index.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbPropertyStructureWorkspaceContext<
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbEntityWorkspaceContext {
	readonly structure: UmbContentTypeStructureManager<ContentTypeModel>;
	// TODO: propertyStructureById is not used by anything in the codebase, should we remove it? [NL]
	propertyStructureById(id: string): Promise<Observable<UmbPropertyTypeModel | undefined>>;
}
