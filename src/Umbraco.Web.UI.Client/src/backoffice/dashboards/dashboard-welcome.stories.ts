import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbDashboardWelcomeElement } from './dashboard-welcome.element';
import './dashboard-welcome.element';

export default {
	title: 'Dashboards/Welcome',
	component: 'umb-dashboard-welcome',
	id: 'umb-dashboard-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardWelcomeElement> = () =>
	html` <umb-dashboard-welcome></umb-dashboard-welcome>`;
AAAOverview.storyName = 'Overview';
