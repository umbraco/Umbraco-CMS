import type { UmbDocumentTypeEntityType } from './entity.js';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { ContentTypeCleanupBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbDocumentTypeEntityType;
	allowedTemplates: Array<{ id: string }>;
	defaultTemplate: { id: string } | null;
	cleanup: ContentTypeCleanupBaseModel;
}
