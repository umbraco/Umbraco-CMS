// TODO: add the missing fields to the type
export type UmbBlockGridPropertyEditorConfig = Array<{
	alias: 'blocks';
	value: Array<{
		allowAtRoot: boolean;
		allowInAreas: boolean;
		contentElementTypeKey: string;
	}>;
}>;
