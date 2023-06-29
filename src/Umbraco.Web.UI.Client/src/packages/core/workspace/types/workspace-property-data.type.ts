import { type UmbDataTypeConfig } from '../../property-editor/index.js';

export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: UmbDataTypeConfig; // This could potentially then come from hardcoded JS object and not the DataType store.
};
