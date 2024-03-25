import type { UmbSaveableWorkspaceContext } from './saveable-workspace-context.interface.js';

export interface UmbPublishableWorkspaceContext extends UmbSaveableWorkspaceContext {
	saveAndPublish(): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
