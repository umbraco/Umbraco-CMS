import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardPublishedStatusElement } from './dashboard-published-status.element';
import './dashboard-published-status.element';

export default {
	title: 'Dashboards/Published Status',
	component: 'umb-dashboard-published-status',
	id: 'umb-dashboard-published-status',
} as Meta;

export const AAAOverview: Story<UmbDashboardPublishedStatusElement> = () =>
	html` <umb-dashboard-published-status></umb-dashboard-published-status>`;
AAAOverview.storyName = 'Overview';
