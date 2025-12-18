// Default mock data set - implements UmbMockDataSet interface
import type { UmbMockDataSet } from '../../types/mock-data-set.types.js';

// Import all data
import { data as dataType } from './data-type.data.js';
import { data as dictionary } from './dictionary.data.js';
import { data as document } from './document.data.js';
import { data as documentBlueprint } from './document-blueprint.data.js';
import { data as documentType } from './document-type.data.js';
import { data as language } from './language.data.js';
import { data as media } from './media.data.js';
import { data as mediaType } from './media-type.data.js';
import { data as member } from './member.data.js';
import { data as memberGroup } from './member-group.data.js';
import { data as memberType } from './member-type.data.js';
import { data as partialView, snippets as partialViewSnippets } from './partial-view.data.js';
import { data as relation } from './relation.data.js';
import { data as relationType } from './relationType.data.js';
import { data as script } from './script.data.js';
import { data as staticFile } from './static-file.data.js';
import { data as stylesheet } from './stylesheet.data.js';
import {
	data as template,
	createTemplateScaffold,
	templateQueryResult,
	templateQuerySettings,
} from './template.data.js';
import { data as user, mfaLoginProviders } from './user.data.js';
import { data as userGroup } from './user-group.data.js';
import { data as objectType } from './object-type.data.js';
import {
	savedSearches as logViewerSavedSearches,
	messageTemplates as logViewerMessageTemplates,
	logLevels as logViewerLogLevels,
} from './log-viewer.data.js';
import { data as logs } from './logs.data.js';
import { data as auditLogs } from './audit-log.data.js';
import {
	healthGroups,
	healthGroupsWithoutResult,
	getGroupByName,
	getGroupWithResultsByName,
} from './health-check.data.js';
import {
	Indexers as examineIndexers,
	PagedIndexers as examinePagedIndexers,
	searchResultMockData as examineSearchResults,
	getIndexByName as examineGetIndexByName,
	getSearchResultsMockData as examineGetSearchResults,
} from './examine.data.js';
import { items as trackedReferenceItems } from './tracked-reference.data.js';
import { data as news } from './umbraco-news.data.js';

// Named exports for all data
export {
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

// Type assertion to ensure this module satisfies UmbMockDataSet
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
