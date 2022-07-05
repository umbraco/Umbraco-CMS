export interface DataTypeEntity {
	id: number;
	key: string;
	name: string;
	//configUI: any; // this is the prevalues...
	propertyEditorUIAlias: string;
}

export const data: Array<DataTypeEntity> = [
	{
		id: 1245,
		key: 'dt-1',
		name: 'Text',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Text',
	},
	{
		id: 1244,
		key: 'dt-2',
		name: 'Textarea',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Textarea',
	},
	{
		id: 1246,
		key: 'dt-3',
		name: 'My JS Property Editor',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
	},
	{
		id: 1247,
		key: 'dt-4',
		name: 'Context Example',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContextExample',
	},
];

// Temp mocked database
class UmbDataTypeData {
	private _data: Array<DataTypeEntity> = [];

	constructor() {
		this._data = data;
	}

	getById(id: number) {
		return this._data.find((item) => item.id === id);
	}

	getByKey(key: string) {
		return this._data.find((item) => item.key === key);
	}

	save(nodes: DataTypeEntity[]) {
		nodes.forEach((node) => {
			const foundIndex = this._data.findIndex((item) => item.id === node.id);
			if (foundIndex !== -1) {
				// replace
				this._data[foundIndex] = node;
			} else {
				// new
				this._data.push(node);
			}
		});
		//console.log('save:', nodes);
		return nodes;
	}
}

export const umbDataTypeData = new UmbDataTypeData();
