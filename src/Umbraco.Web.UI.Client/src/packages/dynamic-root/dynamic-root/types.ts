/**
 * A start node resolved against the content being edited, rather than fixed on the data type.
 */
export interface UmbDynamicRoot {
	originAlias: string;
	originKey?: string;
	querySteps?: Array<UmbDynamicRootQueryStep>;
}

export interface UmbDynamicRootQueryStep {
	unique: string;
	alias: string;
	anyOfDocTypeKeys?: Array<string>;
}
