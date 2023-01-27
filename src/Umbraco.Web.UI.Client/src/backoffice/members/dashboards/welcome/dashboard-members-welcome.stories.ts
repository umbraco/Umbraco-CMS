import './dashboard-members-welcome.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardMembersWelcomeElement } from './dashboard-members-welcome.element';

export default {
	title: 'Dashboards/Welcome',
	component: 'umb-dashboard-welcome',
	id: 'umb-dashboard-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardMembersWelcomeElement> = () =>
	html` <umb-dashboard-members-welcome></umb-dashboard-members-welcome>`;
AAAOverview.storyName = 'Overview';
