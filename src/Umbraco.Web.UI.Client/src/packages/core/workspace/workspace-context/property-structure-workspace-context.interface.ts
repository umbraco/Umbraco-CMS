import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbPropertyStructureWorkspaceContextInterface extends UmbWorkspaceContextInterface {
	propertyStructureById(id: string): Promise<Observable<ValueModelBaseModel | undefined>>;
}
