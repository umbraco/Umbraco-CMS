import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbWorkspaceUniqueType } from './../../types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbEntityWorkspaceContext extends UmbWorkspaceContext {
	unique: Observable<UmbWorkspaceUniqueType | undefined>;
	getUnique(): UmbWorkspaceUniqueType | undefined;
}
