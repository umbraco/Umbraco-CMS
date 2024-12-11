import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';

export interface UmbPublishableWorkspaceContext extends UmbSubmittableWorkspaceContext {
	saveAndPublish(): Promise<void>;
	unpublish(): Promise<void>;
}
