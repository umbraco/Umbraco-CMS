import type { UmbDataTypeEntityType } from './entity.js';

export interface UmbDataTypeDetailModel {
	entityType: UmbDataTypeEntityType;
	unique: string;
	name: string;
	editorAlias: string | undefined;
	editorUiAlias: string;
	values: Array<UmbDataTypePropertyModel>;
}

export interface UmbDataTypePropertyModel {
	alias: string;
	value: any;
}
