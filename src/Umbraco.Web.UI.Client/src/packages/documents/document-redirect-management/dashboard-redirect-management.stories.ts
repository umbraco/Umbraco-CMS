import './dashboard-redirect-management.element.js';

import type { UmbDashboardRedirectManagementElement } from './dashboard-redirect-management.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Redirect Management',
	component: 'umb-dashboard-redirect-management',
	id: 'umb-dashboard-redirect-management',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardRedirectManagementElement> = () =>
	html` <umb-dashboard-redirect-management></umb-dashboard-redirect-management>`;
AAAOverview.storyName = 'Overview';
