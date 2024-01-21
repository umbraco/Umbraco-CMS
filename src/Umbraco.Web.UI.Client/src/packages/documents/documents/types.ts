import { UmbDocumentEntityType } from './entity.js';

export interface UmbDocumentDetailModel {
	unique: string;
	parentUnique: string;
	entityType: UmbDocumentEntityType;
}
