import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbEntityWorkspaceContextInterface extends UmbWorkspaceContextInterface {
	readonly unique: Observable<string | null | undefined>;
	getEntityType(): string;
	getUnique(): string | undefined;
}
