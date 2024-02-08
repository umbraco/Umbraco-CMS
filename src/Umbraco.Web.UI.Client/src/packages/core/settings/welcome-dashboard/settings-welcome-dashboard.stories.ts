import type { Meta, Story } from '@storybook/web-components';
import type { UmbSettingsWelcomeDashboardElement } from './settings-welcome-dashboard.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './settings-welcome-dashboard.element.js';

export default {
	title: 'Dashboards/Settings Welcome',
	component: 'umb-dashboard-settings-welcome',
	id: 'umb-dashboard-settings-welcome',
} as Meta;

export const AAAOverview: Story<UmbSettingsWelcomeDashboardElement> = () =>
	html` <umb-settings-welcome-dashboard></umb-settings-welcome-dashboard>`;
AAAOverview.storyName = 'Overview';
