import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';

export interface UmbPublishableWorkspaceContextInterface<EntityType = unknown>
	extends UmbSaveableWorkspaceContextInterface<EntityType> {
	//getData(): EntityType | undefined;
	saveAndPublish(): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
