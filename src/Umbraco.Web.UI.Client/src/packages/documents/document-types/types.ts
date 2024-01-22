import { UmbDocumentTypeEntityType } from './entity.js';
import { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { ContentTypeCleanupModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbDocumentTypeEntityType;
	allowedTemplates: Array<{ id: string }>;
	defaultTemplate: { id: string } | null;
	cleanup: ContentTypeCleanupModel;
}
