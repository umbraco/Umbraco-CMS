/**
 * A start node resolved against the content being edited, rather than fixed on the data type.
 */
export interface UmbDynamicRoot {
	originAlias: string;
	originKey?: string;
	querySteps?: Array<UmbDynamicRootQueryStep>;
}

/**
 * A query step applied to the resolved origin. `anyOfDocTypeKeys` limits the step to nodes of the listed content types.
 */
export interface UmbDynamicRootQueryStep {
	unique: string;
	alias: string;
	anyOfDocTypeKeys?: Array<string>;
}
