import type { UmbWorkspaceContext } from './workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbEntityWithContentTypeWorkspaceContext extends UmbWorkspaceContext {
	
	/**
	 * Unique identifier for the entities content type
	 */
	readonly contentTypeUnique : Observable<string | undefined>;
}
