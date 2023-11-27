import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbMediaTypeDetailModel = MediaTypeResponseModel & { entityType: typeof UMB_MEDIA_TYPE_ENTITY_TYPE };
