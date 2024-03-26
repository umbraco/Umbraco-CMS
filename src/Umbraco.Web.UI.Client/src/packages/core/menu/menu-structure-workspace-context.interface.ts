import type { UmbStructureItemModel } from './types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbMenuStructureWorkspaceContext {
	structure: Observable<UmbStructureItemModel[]>;
}
