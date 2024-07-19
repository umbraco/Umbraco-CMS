export interface UmbContentTypeCompositionReferenceModel {
	unique: string;
	name: string;
	icon: string;
}

export interface UmbContentTypeAvailableCompositionRequestModel {
	unique: string | null;
	currentPropertyAliases: Array<string>;
	currentCompositeUniques: Array<string>;
}

export interface UmbContentTypeCompositionCompatibleModel {
	unique: string;
	name: string;
	icon: string;
	folderPath: Array<string>;
	isCompatible: boolean;
}
