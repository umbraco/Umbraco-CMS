import { UmbData } from './data';

export interface DocumentTypeEntity {
	id: number;
	key: string;
	name: string;
	alias: string;
	properties: [];
}

export const data: Array<DocumentTypeEntity> = [
	{
		id: 99,
		key: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		name: 'Document Type 1',
		alias: 'documentType1',
		properties: [],
	},
	{
		id: 100,
		key: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		name: 'Document Type 2',
		alias: 'documentType2',
		properties: [],
	},
];

// Temp mocked database
class UmbDocumentTypeData extends UmbData<DocumentTypeEntity> {
	constructor() {
		super(data);
	}
}

export const umbDocumentTypeData = new UmbDocumentTypeData();
