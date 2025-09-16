export interface UmbInteractionMemoryModel {
	clientTimestamp?: string;
	value?: any;
	unique: string;
	memories?: Array<UmbInteractionMemoryModel>;
}
