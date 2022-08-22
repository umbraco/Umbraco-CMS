import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbDashboardRedirectManagementElement } from './dashboard-redirect-management.element';
import './dashboard-redirect-management.element';

export default {
	title: 'Dashboards/Redirect Management',
	component: 'umb-dashboard-redirect-management',
	id: 'umb-dashboard-redirect-management',
} as Meta;

export const AAAOverview: Story<UmbDashboardRedirectManagementElement> = () =>
	html` <umb-dashboard-redirect-management></umb-dashboard-redirect-management>`;
AAAOverview.storyName = 'Overview';
