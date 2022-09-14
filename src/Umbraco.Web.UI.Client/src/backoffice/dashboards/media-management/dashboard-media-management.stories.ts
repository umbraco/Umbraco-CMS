import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardMediaManagementElement } from './dashboard-media-management.element';
import './dashboard-media-management.element';

export default {
	title: 'Dashboards/Media Management',
	component: 'umb-dashboard-media-management',
	id: 'umb-dashboard-media-management',
} as Meta;

export const AAAOverview: Story<UmbDashboardMediaManagementElement> = () =>
	html` <umb-dashboard-media-management></umb-dashboard-media-management>`;
AAAOverview.storyName = 'Overview';
