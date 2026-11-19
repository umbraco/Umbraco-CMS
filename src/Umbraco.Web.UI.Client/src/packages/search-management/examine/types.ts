import type { DocumentModel, FieldModel, IndexDocumentModel } from './api/index.js';

export type UmbExamineFieldModel = FieldModel;

export type UmbExamineIndexDocumentModel = IndexDocumentModel;

export type UmbExamineDocumentModel = DocumentModel;

export interface UmbExamineShowFieldsModalData {
	documentUnique: string;
	indexAlias: string;
	culture?: string;
}
