export const dataType = [];
export const dictionary = [];
export const document = [];
export const documentBlueprint = [];
export const documentType = [];
export const language = [];
export const media = [];
export const mediaType = [];
export const member = [];
export const memberGroup = [];
export const memberType = [];
export const partialView = [];
export const partialViewSnippets = [];
export const relation = [];
export const relationType = [];
export const script = [];
export const staticFile = [];
export const stylesheet = [];
export const template = [];
export { data as user } from '../default/user.data.js';
export { data as userGroup } from '../default/user-group.data.js';
export const objectType = [];
export const logViewerSavedSearches = [];
export const logViewerMessageTemplates = [];
export const logViewerLogLevels = {
	total: 0,
	items: [] as Array<{ name: string; level: string }>,
};
export const logs = [];
export const auditLogs = [];
export const healthGroups = [];
export const healthGroupsWithoutResult = [];
export const getGroupByName = (_name: string) => undefined;
export const getGroupWithResultsByName = (_name: string) => undefined;
export const examineIndexers = [];
export const examinePagedIndexers = { items: [], total: 0 };
export const examineSearchResults = [];
export const examineGetIndexByName = (_indexName: string) => undefined;
export const examineGetSearchResults = () => [];
export const trackedReferenceItems = [];
export const news = [];

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
