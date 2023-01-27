import type { Observable } from "rxjs";
import { UmbEntityDetailStore } from "@umbraco-cms/store";

export interface UmbWorkspaceEntityContextInterface<T = unknown> {


	readonly data: Observable<T>;
	readonly name: Observable<string|undefined>;

	//entityKey?: string;
	//entityType: string;

	getEntityKey(): string | undefined;
	getEntityType(): string;

	getData(): T;

	load(entityKey: string): void;

	create(parentKey: string | null): void;

	getStore(): UmbEntityDetailStore<T> | undefined;

	setPropertyValue(alias: string, value: unknown): void;

	save(): Promise<void>;

	destroy(): void;
}
