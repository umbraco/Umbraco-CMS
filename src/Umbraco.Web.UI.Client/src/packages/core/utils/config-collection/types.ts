export interface UmbConfigCollectionEntryModel {
	alias: string;
	value: unknown;
}

export type UmbConfigCollectionModel = Array<UmbConfigCollectionEntryModel>;
