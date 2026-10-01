/**
 * A start node resolved against the content being edited, rather than fixed on the data type.
 *
 * Any picker of content can offer a dynamic root; resolving it is done by `UmbDynamicRootResolver`.
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
