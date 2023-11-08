export interface UmbWorkspaceContextInterface {
	readonly workspaceAlias: string;
	// TODO: should we consider another name than entity type. File system files are not entities but still have this type.
	getEntityType(): string;
	getEntityId(): string | undefined; // Consider if this should go away now that we have getUnique()
}
