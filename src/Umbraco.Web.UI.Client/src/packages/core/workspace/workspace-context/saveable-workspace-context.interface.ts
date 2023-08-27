import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';

export interface UmbSaveableWorkspaceContextInterface<EntityType = unknown>
	extends UmbWorkspaceContextInterface<EntityType> {
	//getData(): EntityType | undefined;
	save(): Promise<void>;
}
