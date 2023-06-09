import './dashboard-members-welcome.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbDashboardMembersWelcomeElement } from './dashboard-members-welcome.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Welcome',
	component: 'dashboard-members-welcome',
	id: 'umb-dashboard-members-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardMembersWelcomeElement> = () =>
	html` <umb-dashboard-members-welcome></umb-dashboard-members-welcome>`;
AAAOverview.storyName = 'Overview';
