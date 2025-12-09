import { dataSet } from '../sets/index.js';
import type {
	DocumentTypeItemResponseModel,
	DocumentTypeResponseModel,
	DocumentTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDocumentTypeModel = DocumentTypeResponseModel &
	DocumentTypeTreeItemResponseModel &
	DocumentTypeItemResponseModel;

export const data: Array<UmbMockDocumentTypeModel> = dataSet.documentType;
