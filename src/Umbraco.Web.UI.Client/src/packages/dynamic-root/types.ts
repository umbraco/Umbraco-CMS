/**
 * A start node resolved against the content being edited, rather than fixed on the data type.
 *
 * Named for the content picker for historical reasons: the multi node tree picker was the only editor to
 * offer a dynamic root when these types were introduced, and the names are public API.
 */
export interface UmbContentPickerDynamicRoot {
	originAlias: string;
	originKey?: string;
	querySteps?: Array<UmbContentPickerDynamicRootQueryStep>;
}

export interface UmbContentPickerDynamicRootQueryStep {
	unique: string;
	alias: string;
	anyOfDocTypeKeys?: Array<string>;
}
