import { dataSet } from '../sets/index.js';
import type {
	DocumentItemResponseModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDocumentModel = DocumentResponseModel & DocumentTreeItemResponseModel & DocumentItemResponseModel;

export const data: Array<UmbMockDocumentModel> = dataSet.document;
