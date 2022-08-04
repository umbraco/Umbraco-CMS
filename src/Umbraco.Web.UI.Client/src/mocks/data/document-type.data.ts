export interface DocumentTypeEntity {
	id: number;
	key: string;
	name: string;
	properties: [];
}

export const data: Array<DocumentTypeEntity> = [
	{
		id: 99,
		key: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		name: 'Document Type 1',
		properties: [],
	},
	{
		id: 100,
		key: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		name: 'Document Type 2',
		properties: [],
	},
];

// Temp mocked database
class UmbDocumentTypeData {
	private _data: Array<DocumentTypeEntity> = [];

	constructor() {
		this._data = data;
	}

	getById(id: number) {
		return this._data.find((item) => item.id === id);
	}

	save(nodes: DocumentTypeEntity[]) {
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
		return nodes;
	}
}

export const umbDocumentTypeData = new UmbDocumentTypeData();
