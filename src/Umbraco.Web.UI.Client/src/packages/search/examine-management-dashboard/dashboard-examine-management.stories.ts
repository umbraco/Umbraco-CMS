import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbDashboardExamineManagementElement } from './dashboard-examine-management.element.js';
import './dashboard-examine-management.element';

import type { UmbDashboardExamineOverviewElement } from './views/section-view-examine-overview.js';
import './views/section-view-examine-overview';

import type { UmbDashboardExamineIndexElement } from './views/section-view-examine-indexers.js';
import './views/section-view-examine-indexers';

import type { UmbDashboardExamineSearcherElement } from './views/section-view-examine-searchers.js';
import './views/section-view-examine-searchers';

export default {
	title: 'Dashboards/Examine Management',
	component: 'umb-dashboard-examine-management',
	id: 'umb-dashboard-examine-management',
} as Meta;

export const AAAOverview: Story<UmbDashboardExamineOverviewElement> = () =>
	html` <umb-dashboard-examine-overview></umb-dashboard-examine-overview>`;
AAAOverview.storyName = 'Overview';

export const Index: Story<UmbDashboardExamineIndexElement> = () =>
	html` <umb-dashboard-examine-index indexName="InternalIndex"></umb-dashboard-examine-index>`;

export const Searcher: Story<UmbDashboardExamineSearcherElement> = () =>
	html` <umb-dashboard-examine-searcher searcherName="InternalSearcher"></umb-dashboard-examine-searcher>`;
