import type { UmbWorkspaceRouteManager } from '../index.js';
import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';

export interface UmbRoutableWorkspaceContext extends UmbWorkspaceContextInterface {
	readonly routes: UmbWorkspaceRouteManager;
}
