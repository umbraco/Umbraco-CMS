import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';

export interface UmbSaveableWorkspaceContext extends UmbSubmittableWorkspaceContext {
	requestSave(): Promise<void>;
}
