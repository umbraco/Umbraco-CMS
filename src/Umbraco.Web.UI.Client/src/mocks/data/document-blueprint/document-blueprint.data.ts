import { dataSet } from '../sets/index.js';
import type {
	DocumentBlueprintItemResponseModel,
	DocumentBlueprintResponseModel,
	DocumentBlueprintTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDocumentBlueprintModel = DocumentBlueprintResponseModel &
	DocumentBlueprintItemResponseModel &
	DocumentBlueprintTreeItemResponseModel;

export const data: Array<UmbMockDocumentBlueprintModel> = dataSet.documentBlueprint;
