import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';

export interface UmbEntityWorkspaceContextInterface<EntityType = unknown>
	extends UmbWorkspaceContextInterface<EntityType> {
	getEntityType(): string; // TODO: consider of this should be on the repository because a repo is responsible for one entity type
	//getData(): EntityType | undefined;
	save(): Promise<void>;
}
