import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardExamineManagementElement } from './dashboard-examine-management.element';
import './dashboard-examine-management.element';

export default {
	title: 'Dashboards/Examine Management',
	component: 'umb-dashboard-examine-management',
	id: 'umb-dashboard-examine-management',
} as Meta;

export const AAAOverview: Story<UmbDashboardExamineManagementElement> = () =>
	html` <umb-dashboard-examine-management></umb-dashboard-examine-management>`;
AAAOverview.storyName = 'Overview';
