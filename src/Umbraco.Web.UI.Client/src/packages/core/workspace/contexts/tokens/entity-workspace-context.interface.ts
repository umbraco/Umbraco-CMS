import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbWorkspaceUniqueType } from './../../types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbEntityWorkspaceContextInterface extends UmbWorkspaceContextInterface {
	unique: Observable<UmbWorkspaceUniqueType | undefined>;
	getUnique(): UmbWorkspaceUniqueType | undefined;
}
