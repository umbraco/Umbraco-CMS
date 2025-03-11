import type { UmbDataTypeEntityType } from './entity.js';

export type * from './entity.js';
export interface UmbDataTypeDetailModel {
	entityType: UmbDataTypeEntityType;
	unique: string;
	name: string;
	editorAlias: string | undefined;
	editorUiAlias: string | null;
	values: Array<UmbDataTypePropertyValueModel>;
}

export interface UmbDataTypePropertyValueModel<ValueType = unknown> {
	alias: string;
	value: ValueType;
}
