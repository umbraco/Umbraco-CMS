import type { UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

export interface UmbEntityWorkspaceContextInterface<EntityType = unknown>
	extends UmbWorkspaceContextInterface<EntityType> {
	getEntityKey(): string | undefined; // COnsider if this should go away now that we have getUnique()
	getEntityType(): string; // TODO: consider of this should be on the repository because a repo is responsible for one entity type
	//getData(): EntityType | undefined;
	save(): Promise<void>;
}
