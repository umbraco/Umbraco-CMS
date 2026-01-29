// Test mock data set - implements UmbMockDataSet interface with empty/minimal data
// Uses 'as never[]' to satisfy type constraints while keeping arrays empty
import type { UmbMockDataSet } from '../../types/mock-data-set.types.js';

export const dataType = [] as never[];
export const dictionary = [] as never[];
export const document = [] as never[];
export const documentBlueprint = [] as never[];
export const documentType = [] as never[];
export const language = [] as never[];
export const media = [] as never[];
export const mediaType = [] as never[];
export const member = [] as never[];
export const memberGroup = [] as never[];
export const memberType = [] as never[];
export const partialView = [] as never[];
export const partialViewSnippets = [] as never[];
export const relation = [] as never[];
export const relationType = [] as never[];
export const script = [] as never[];
export const staticFile = [] as never[];
export const stylesheet = [] as never[];
export const template = [] as never[];
export { data as user } from '../default/user.data.js';
export { data as userGroup } from '../default/user-group.data.js';
export const objectType = [] as never[];
export const logViewerSavedSearches = [] as never[];
export const logViewerMessageTemplates = [] as never[];
export const logViewerLogLevels = {
	total: 0,
	items: [] as Array<{ name: string; level: string }>,
};
export const logs = [] as never[];
export const auditLogs = [] as never[];
export const healthGroups = [] as never[];
export const healthGroupsWithoutResult = [] as never[];
export const getGroupByName = (_name: string) => undefined;
export const getGroupWithResultsByName = (_name: string) => undefined;
export const examineIndexers = [] as never[];
export const examinePagedIndexers = { items: [], total: 0 };
export const examineSearchResults = [] as never[];
export const examineGetIndexByName = (_indexName: string) => undefined;
export const examineGetSearchResults = () => [] as never[];
export const trackedReferenceItems = [] as never[];
export const news = [] as never[];

// Template-specific exports
export const createTemplateScaffold = (masterTemplateAlias: string) => {
	return `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
	Layout = ${masterTemplateAlias};
}`;
};

export const templateQueryResult = {
	queryExpression: '',
	sampleResults: [],
	resultCount: 0,
	executionTime: 0,
};

export const templateQuerySettings = {
	documentTypeAliases: [],
	properties: [],
	operators: [],
};

// Type assertion to ensure this module satisfies UmbMockDataSet
// Note: This will fail at compile-time if the module doesn't satisfy the interface
// We import user and userGroup from the default set for authentication testing
import { data as user } from '../default/user.data.js';
import { data as mfaLoginProviders } from '../default/user-mfa-login-providers.data.js';
import { data as userGroup } from '../default/user-group.data.js';

const _typeCheck: UmbMockDataSet = {
	dataType,
	dictionary,
	document,
	documentBlueprint,
	documentType,
	language,
	media,
	mediaType,
	member,
	memberGroup,
	memberType,
	partialView,
	partialViewSnippets,
	relation,
	relationType,
	script,
	staticFile,
	stylesheet,
	template,
	createTemplateScaffold,
	templateQueryResult,
	templateQuerySettings,
	user,
	mfaLoginProviders,
	userGroup,
	objectType,
	logViewerSavedSearches,
	logViewerMessageTemplates,
	logViewerLogLevels,
	logs,
	auditLogs,
	healthGroups,
	healthGroupsWithoutResult,
	getGroupByName,
	getGroupWithResultsByName,
	examineIndexers,
	examinePagedIndexers,
	examineSearchResults,
	examineGetIndexByName,
	examineGetSearchResults,
	trackedReferenceItems,
	news,
};
void _typeCheck;
