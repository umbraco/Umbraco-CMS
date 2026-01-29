import { dataSet } from '../data/sets/index.js';
import { UmbMockDBBase } from './utils/mock-db-base.js';
import type {
	LogMessageResponseModel,
	LogTemplateResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbLogViewerSearchesData extends UmbMockDBBase<SavedLogSearchResponseModel> {
	constructor(data: SavedLogSearchResponseModel[]) {
		super(data);
	}

	getSavedSearches(skip = 0, take = this.data.length): Array<SavedLogSearchResponseModel> {
		return this.data.slice(skip, take + skip);
	}

	getByName(name: string) {
		return this.data.find((search) => search.name === name);
	}
}

class UmbLogViewerTemplatesData extends UmbMockDBBase<LogTemplateResponseModel> {
	constructor(data: LogTemplateResponseModel[]) {
		super(data);
	}

	getTemplates(skip = 0, take = this.data.length): Array<LogTemplateResponseModel> {
		return this.data.slice(skip, take + skip);
	}
}

class UmbLogViewerMessagesData extends UmbMockDBBase<LogMessageResponseModel> {
	constructor(data: LogMessageResponseModel[]) {
		super(data);
	}

	getLogs(skip = 0, take = this.data.length): Array<LogMessageResponseModel> {
		return this.data.slice(skip, take);
	}

	getLevelCount(): Record<string, number> {
		const levels = this.data.reduce(
			(counts, log) => {
				const level = log.level?.toLocaleLowerCase() ?? 'unknown';
				counts[level] = (counts[level] || 0) + 1;
				return counts;
			},
			{} as Record<string, number>,
		);

		// Test 1k logs for the first level
		levels[Object.keys(levels)[0]] += 1000;

		// Test 1m logs for the second level
		levels[Object.keys(levels)[1]] += 1000000;

		return levels;
	}
}

const defaultLogLevels = {
	total: 0,
	items: [] as Array<{ name: string; level: string }>,
};

export const umbLogViewerData = {
	searches: new UmbLogViewerSearchesData(dataSet.logViewerSavedSearches ?? []),
	templates: new UmbLogViewerTemplatesData(dataSet.logViewerMessageTemplates ?? []),
	logs: new UmbLogViewerMessagesData(dataSet.logs ?? []),
	logLevels: dataSet.logViewerLogLevels ?? defaultLogLevels,
};
