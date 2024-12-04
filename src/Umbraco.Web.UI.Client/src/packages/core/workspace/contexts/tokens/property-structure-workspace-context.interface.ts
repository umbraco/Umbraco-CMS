import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbPropertyStructureWorkspaceContext<
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbEntityWorkspaceContext {
	readonly structure: UmbContentTypeStructureManager<ContentTypeModel>;
	// TODO: propertyStructureById is not used by anything in the codebase, should we remove it? [NL]
	propertyStructureById(id: string): Promise<Observable<UmbPropertyTypeModel | undefined>>;
}
