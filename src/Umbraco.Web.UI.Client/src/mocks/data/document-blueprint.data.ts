import type { DocumentBlueprintDetails } from '../../packages/documents/document-blueprints/types';
import { UmbEntityData } from './entity.data';

export const data: Array<DocumentBlueprintDetails> = [
	{
		name: 'Document Blueprint 1',
		type: 'document-blueprint',
		id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
		icon: 'umb:blueprint',
		documentTypeKey: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		properties: [],
		data: [],
	},
	{
		name: 'Document Blueprint 2',
		type: 'document-blueprint',
		id: '3fa85f64-5717-4562-b3qc-2c963f66afa6',
		icon: 'umb:blueprint',
		documentTypeKey: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		properties: [],
		data: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDocumentBlueprintData extends UmbEntityData<DocumentBlueprintDetails> {
	constructor() {
		super(data);
	}
}

export const umbDocumentBlueprintData = new UmbDocumentBlueprintData();
