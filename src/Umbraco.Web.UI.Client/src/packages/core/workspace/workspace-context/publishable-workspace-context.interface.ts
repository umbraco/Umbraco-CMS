import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';

export interface UmbPublishableWorkspaceContextInterface extends UmbSaveableWorkspaceContextInterface {
	saveAndPublish(): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
