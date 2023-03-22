import './dashboard-welcome.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardWelcomeElement } from './dashboard-welcome.element';

export default {
	title: 'Dashboards/Welcome',
	component: 'umb-dashboard-welcome',
	id: 'umb-dashboard-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardWelcomeElement> = () =>
	html` <umb-dashboard-welcome></umb-dashboard-welcome>`;
AAAOverview.storyName = 'Overview';
