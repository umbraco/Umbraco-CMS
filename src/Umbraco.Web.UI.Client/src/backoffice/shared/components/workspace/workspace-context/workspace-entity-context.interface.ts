import type { UmbWorkspaceContextInterface } from './workspace-context.interface';

export interface UmbEntityWorkspaceContextInterface<T = unknown> extends UmbWorkspaceContextInterface<T> {
	getEntityKey(): string | undefined; // COnsider if this should go away now that we have getUnique()
	getEntityType(): string; // TODO: consider of this should be on the repository because a repo is responsible for one entity type
	getData(): T;
	save(): Promise<void>;
}
