import type { Observable } from 'rxjs';
import { UmbWorkspaceContextInterface } from './workspace-context.interface';
import { UmbEntityDetailStore } from '@umbraco-cms/store';

export interface UmbWorkspaceEntityContextInterface<T = unknown> extends UmbWorkspaceContextInterface<T> {
	readonly name: Observable<string | undefined>;

	getEntityKey(): string | undefined; // COnsider if this should go away now that we have getUnique()
	getEntityType(): string;

	getData(): T;

	getStore(): UmbEntityDetailStore<T> | undefined;

	setPropertyValue(alias: string, value: unknown): void;

	save(isNew: boolean): Promise<void>;
}
