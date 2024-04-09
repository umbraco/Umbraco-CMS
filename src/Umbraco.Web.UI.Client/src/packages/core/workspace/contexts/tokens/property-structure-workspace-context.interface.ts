import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantPropertyValueModel } from '@umbraco-cms/backoffice/variant';

export interface UmbPropertyStructureWorkspaceContext extends UmbEntityWorkspaceContext {
	propertyStructureById(id: string): Promise<Observable<UmbVariantPropertyValueModel | undefined>>;
}
