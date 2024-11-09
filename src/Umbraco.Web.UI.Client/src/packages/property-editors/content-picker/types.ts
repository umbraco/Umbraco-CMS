export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: UmbContentPickerDynamicRoot;
};

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
