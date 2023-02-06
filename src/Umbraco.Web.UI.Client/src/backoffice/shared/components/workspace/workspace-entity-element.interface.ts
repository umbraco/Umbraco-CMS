export interface UmbWorkspaceEntityElement {
	load(key: string): void;
	create(parentKey: string | null): void;
}
