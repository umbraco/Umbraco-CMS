import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';

export interface UmbPublishableWorkspaceContext extends UmbWorkspaceContext {
	saveAndPublish(): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
