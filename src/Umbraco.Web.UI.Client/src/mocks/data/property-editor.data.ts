import { UmbData } from './data';

export interface PropertyEditor {
	alias: string;
	name: string;
	editor: PropertyEditorEditor;
}

export interface PropertyEditorEditor {
	view: string;
}

export const data: Array<PropertyEditor> = [
	{
		alias: 'Sir.Trevor',
		name: 'Sir Trevor',
		editor: {
			view: '/App_Plugins/SirTrevor/SirTrevor.html',
		},
	},
];

// Temp mocked database
class UmbPropertyEditorData extends UmbData<PropertyEditor> {
	constructor() {
		super(data);
	}

	getAll() {
		return this.data;
	}
}

export const umbPropertyEditorData = new UmbPropertyEditorData();
