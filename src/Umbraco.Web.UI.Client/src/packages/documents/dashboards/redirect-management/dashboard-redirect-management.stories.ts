import './dashboard-redirect-management.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbDashboardRedirectManagementElement } from './dashboard-redirect-management.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Redirect Management',
	component: 'umb-dashboard-redirect-management',
	id: 'umb-dashboard-redirect-management',
} as Meta;

export const AAAOverview: Story<UmbDashboardRedirectManagementElement> = () =>
	html` <umb-dashboard-redirect-management></umb-dashboard-redirect-management>`;
AAAOverview.storyName = 'Overview';
