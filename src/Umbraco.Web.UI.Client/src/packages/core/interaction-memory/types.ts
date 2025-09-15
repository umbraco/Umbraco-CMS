export interface UmbInteractionMemoryModel {
	clientTimestamp?: string;
	value?: UmbInteractionMemoryValueModel;
	unique: string;
	memories?: Array<UmbInteractionMemoryModel>;
}

export interface UmbInteractionMemoryValueModel {
	[key: string]: unknown;
}
