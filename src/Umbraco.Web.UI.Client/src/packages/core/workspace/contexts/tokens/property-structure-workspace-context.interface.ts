import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbPropertyStructureWorkspaceContext extends UmbEntityWorkspaceContext {
	propertyStructureById(id: string): Promise<Observable<ValueModelBaseModel | undefined>>;
}
