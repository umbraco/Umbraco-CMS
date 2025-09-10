export interface UmbInteractionMemoryModel {
	clientTimestamp?: string;
	values?: Array<UmbInteractionMemoryValueModel>;
	unique: string;
	memories?: Array<UmbInteractionMemoryModel>;
}

export interface UmbInteractionMemoryValueModel {
	unique: string;
	[key: string]: unknown;
}
