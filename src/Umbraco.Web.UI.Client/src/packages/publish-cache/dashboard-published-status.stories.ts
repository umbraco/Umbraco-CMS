import type { UmbDashboardPublishedStatusElement } from './dashboard-published-status.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './dashboard-published-status.element.js';

export default {
	title: 'Dashboards/Published Status',
	component: 'umb-dashboard-published-status',
	id: 'umb-dashboard-published-status',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardPublishedStatusElement> = () =>
	html` <umb-dashboard-published-status></umb-dashboard-published-status>`;
AAAOverview.storyName = 'Overview';
