export interface UmbDataTypeDetailModel {
	entityType: string;
	unique: string;
	parentUnique: string | null;
	name: string;
	propertyEditorAlias: string | undefined;
	propertyEditorUiAlias: string | null;
	values: Array<UmbDataTypePropertyModel>;
}

export interface UmbDataTypePropertyModel {
	alias: string;
	value: any;
}
