export interface UmbExamineFieldModel {
	name: string;
	type: string;
	values: Array<string>;
}

export interface UmbExamineIndexDocumentModel {
	fields: Array<UmbExamineFieldModel>;
}

export interface UmbExamineDocumentModel {
	documents: Array<UmbExamineIndexDocumentModel>;
}

export interface UmbExamineShowFieldsModalData {
	documentUnique: string;
	indexAlias: string;
	culture?: string;
}
