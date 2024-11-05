import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbEntityWorkspaceContext extends UmbWorkspaceContext {
	unique: Observable<UmbEntityUnique | undefined>;
	getUnique(): UmbEntityUnique | undefined;
}
