import type { UmbWorkspaceContextInterface } from './workspace-context.interface';

export interface UmbEntityWorkspaceContextInterface<T = unknown> extends UmbWorkspaceContextInterface<T> {
	getEntityKey(): string | undefined; // COnsider if this should go away now that we have getUnique()
	getEntityType(): string;
	getData(): T;
	save(): Promise<void>;
}
