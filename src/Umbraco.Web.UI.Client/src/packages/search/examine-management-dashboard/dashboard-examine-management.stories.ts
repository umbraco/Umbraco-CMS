import type { Meta, StoryFn } from '@storybook/web-components';
//import type { UmbDashboardExamineManagementElement } from './dashboard-examine-management.element.js';
import './dashboard-examine-management.element.js';
import type { UmbDashboardExamineOverviewElement } from './views/section-view-examine-overview.js';
import './views/section-view-examine-overview.js';
import type { UmbDashboardExamineIndexElement } from './views/section-view-examine-indexers.js';
import './views/section-view-examine-indexers.js';
import type { UmbDashboardExamineSearcherElement } from './views/section-view-examine-searchers.js';
import './views/section-view-examine-searchers.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Examine Management',
	component: 'umb-dashboard-examine-management',
	id: 'umb-dashboard-examine-management',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardExamineOverviewElement> = () =>
	html` <umb-dashboard-examine-overview></umb-dashboard-examine-overview>`;
AAAOverview.storyName = 'Overview';

export const Index: StoryFn<UmbDashboardExamineIndexElement> = () =>
	html` <umb-dashboard-examine-index indexName="InternalIndex"></umb-dashboard-examine-index>`;

export const Searcher: StoryFn<UmbDashboardExamineSearcherElement> = () =>
	html` <umb-dashboard-examine-searcher searcherName="InternalSearcher"></umb-dashboard-examine-searcher>`;
