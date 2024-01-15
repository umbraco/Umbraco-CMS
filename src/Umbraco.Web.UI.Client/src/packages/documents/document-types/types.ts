import { UmbDocumentTypeEntityType } from './entity.js';
import { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { ContentTypeCleanupModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbDocumentTypeEntityType;
	allowedTemplateIds: Array<string>;
	defaultTemplateId: string | null;
	cleanup: ContentTypeCleanupModel;
}
