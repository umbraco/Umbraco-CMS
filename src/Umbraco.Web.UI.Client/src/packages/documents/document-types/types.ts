import type { UmbDocumentTypeEntityType } from './entity.js';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { ContentTypeCleanupBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDocumentTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbDocumentTypeEntityType;
	allowedTemplates: Array<{ id: string }>;
	defaultTemplate: { id: string } | null;
	cleanup: ContentTypeCleanupBaseModel;
}

export interface UmbDocumentTypeCompositionRequestModel {
	unique: string;
	//Do we really need to send this to the server - Why isn't unique enough?
	isElement: boolean;
	currentPropertyAliases: Array<string>;
	currentCompositeUniques: Array<string>;
}

export interface UmbDocumentTypeCompositionCompatibleModel {
	unique: string;
	name: string;
	icon: string;
	folderPath: Array<string>;
	isCompatible: boolean;
}

export interface UmbDocumentTypeCompositionReferenceModel {
	unique: string;
	name: string;
	icon: string;
}
