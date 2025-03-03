import type { UmbDashboardHealthCheckOverviewElement } from './views/health-check-overview.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './views/health-check-overview.element.js';

import type { UmbDashboardHealthCheckGroupElement } from './views/health-check-group.element.js';
import './views/health-check-group.element.js';

export default {
	title: 'Dashboards/Health Check',
	component: 'umb-dashboard-health-check-overview',
	id: 'umb-dashboard-health-check',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardHealthCheckOverviewElement> = () =>
	html` <umb-dashboard-health-check-overview></umb-dashboard-health-check-overview>`;
AAAOverview.storyName = 'Overview';

export const Group: StoryFn<UmbDashboardHealthCheckGroupElement> = () =>
	html` <umb-dashboard-health-check-group></umb-dashboard-health-check-group>`;
