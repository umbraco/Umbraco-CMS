import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';

export interface UmbPublishableWorkspaceContext extends UmbSubmittableWorkspaceContext {
	saveAndPublish(): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
