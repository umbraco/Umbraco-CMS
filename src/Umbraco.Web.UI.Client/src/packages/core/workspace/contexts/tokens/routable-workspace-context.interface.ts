import type { UmbWorkspaceRouteManager } from '../../index.js';
import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';

export interface UmbRoutableWorkspaceContext extends UmbWorkspaceContext {
	readonly routes: UmbWorkspaceRouteManager;
}
