import './member-welcome-dashboard.element.js';

import type { Meta, Story } from '@storybook/web-components';
import type { UmbMemberWelcomeDashboardElement } from './member-welcome-dashboard.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Welcome',
	component: 'dashboard-members-welcome',
	id: 'umb-dashboard-members-welcome',
} as Meta;

export const AAAOverview: Story<UmbMemberWelcomeDashboardElement> = () =>
	html` <umb-dashboard-members-welcome></umb-dashboard-members-welcome>`;
AAAOverview.storyName = 'Overview';
