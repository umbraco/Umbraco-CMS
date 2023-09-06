import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbPropertyStructureWorkspaceContextInterface<EntityType = unknown>
	extends UmbWorkspaceContextInterface<EntityType> {

		propertyStructureById(id: string): Promise<Observable<ValueModelBaseModel | undefined>>;

}
