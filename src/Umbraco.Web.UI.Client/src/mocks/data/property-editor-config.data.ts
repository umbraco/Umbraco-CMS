import { UmbData } from './data';

export interface PropertyEditorConfig {
	key: string;
	properties: Array<PropertyEditorConfigProperty>;
	data: Array<PropertyEditorConfigPropertyData>;
}

export interface PropertyEditorConfigProperty {
	alias: string;
	label: string;
	description: string;
	view: string; // TODO: change to custom element
}

export interface PropertyEditorConfigPropertyData {
	alias: string;
	value: any;
}

export const data: Array<PropertyEditorConfig> = [
	{
		key: 'a837ba37-b409-4437-ae50-a583a4f828c9',
		properties: [
			{
				alias: 'maxChars',
				label: 'Maximum allowed characters',
				description: 'If empty, 512 character limit',
				view: 'textstringlimited',
			},
		],
		data: [
			{
				alias: 'maxChars',
				value: null,
			},
		],
	},
];

// Temp mocked database
class UmbDataTypeData extends UmbData<PropertyEditorConfig> {
	constructor() {
		super(data);
	}
}

export const umbDataTypeData = new UmbDataTypeData();
