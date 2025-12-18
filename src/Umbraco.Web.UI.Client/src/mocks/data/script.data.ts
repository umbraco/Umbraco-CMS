import { dataSet } from './sets/index.js';
import type {
	FileSystemTreeItemPresentationModel,
	ScriptItemResponseModel,
	ScriptResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockScriptModel = ScriptResponseModel & FileSystemTreeItemPresentationModel & ScriptItemResponseModel;

export const data: Array<UmbMockScriptModel> = dataSet.script;
