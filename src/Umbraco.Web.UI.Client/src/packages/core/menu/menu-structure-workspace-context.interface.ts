import type { UmbStructureItemModel } from './types.js';
import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbMenuStructureWorkspaceContext extends UmbContext {
	structure: Observable<UmbStructureItemModel[]>;
}
