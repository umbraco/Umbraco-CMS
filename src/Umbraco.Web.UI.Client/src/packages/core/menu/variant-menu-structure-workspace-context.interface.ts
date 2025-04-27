import type { UmbVariantStructureItemModel } from './types.js';
import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbVariantMenuStructureWorkspaceContext extends UmbContext {
	structure: Observable<UmbVariantStructureItemModel[]>;
}
