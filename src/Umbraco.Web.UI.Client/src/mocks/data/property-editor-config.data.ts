import { UmbData } from './data';

export interface PropertyEditorConfig {
	propertyEditorAlias: string;
	properties: Array<PropertyEditorConfigProperty>;
}

export interface PropertyEditorConfigProperty {
	alias: string;
	label: string;
	description: string;
	propertyEditorUI: string;
}

export const data: Array<PropertyEditorConfig> = [
	{
		propertyEditorAlias: 'Umbraco.TextBox',
		properties: [
			{
				alias: 'maxChars',
				label: 'Maximum allowed characters',
				description: 'If empty, 512 character limit',
				propertyEditorUI: 'Umb.PropertyEditorUI.Textarea',
			},
		],
	},
	{
		propertyEditorAlias: 'Umbraco.TextArea',
		properties: [
			{
				alias: 'maxChars',
				label: 'Maximum allowed characters',
				description: 'If empty - no character limit',
				propertyEditorUI: 'Umb.PropertyEditorUI.Number',
			},
			{
				alias: 'rows',
				label: 'Number of rows',
				description: 'If empty - 10 rows would be set as the default value',
				propertyEditorUI: 'Umb.PropertyEditorUI.Number',
			},
		],
	},
];

// Temp mocked database
class UmbPropertyEditorConfigData extends UmbData<PropertyEditorConfig> {
	constructor() {
		super(data);
	}

	getByAlias(alias: string) {
		return this.data.find((x) => x.propertyEditorAlias === alias);
	}
}

export const umbPropertyEditorConfigData = new UmbPropertyEditorConfigData();
