import { dataSet } from '../sets/index.js';
import type {
	LogTemplateResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const savedSearches: Array<SavedLogSearchResponseModel> = dataSet.logViewerSavedSearches;

export const messageTemplates: LogTemplateResponseModel[] = dataSet.logViewerMessageTemplates;

export const logLevels = dataSet.logViewerLogLevels;
