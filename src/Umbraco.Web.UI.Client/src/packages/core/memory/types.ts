export interface UmbMemoryModel {
	clientTimestamp?: string;
	values?: Array<UmbMemoryValueModel>;
	unique: string;
}

export interface UmbMemoryValueModel {
	unique: string;
	[key: string]: unknown;
}
