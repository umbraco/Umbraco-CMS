import { Observable } from "rxjs";

export interface UmbWorkspaceEntityContextInterface<T> {

	readonly data:Observable<T>;
	readonly name:Observable<string>;

	entityKey?: string;
	entityType: string;


	getData(): T;

	load(entityKey: string): void;

	create(parentKey: string | null): void;

	getStore(): unknown;

	setPropertyValue(alias: string, value: unknown): void;

	save(): Promise<void>;

	destroy(): void;
}
