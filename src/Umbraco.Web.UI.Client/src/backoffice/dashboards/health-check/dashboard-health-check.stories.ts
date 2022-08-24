import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardHealthCheckElement } from './dashboard-health-check.element';
import './dashboard-health-check.element';

export default {
	title: 'Dashboards/Health Check',
	component: 'umb-dashboard-health-check',
	id: 'umb-dashboard-health-check',
} as Meta;

export const AAAOverview: Story<UmbDashboardHealthCheckElement> = () =>
	html` <umb-dashboard-health-check></umb-dashboard-health-check>`;
AAAOverview.storyName = 'Overview';
