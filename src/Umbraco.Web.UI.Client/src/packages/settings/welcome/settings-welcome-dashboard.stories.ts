import type { UmbSettingsWelcomeDashboardElement } from './settings-welcome-dashboard.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './settings-welcome-dashboard.element.js';

export default {
	title: 'Dashboards/Settings Welcome',
	component: 'umb-dashboard-settings-welcome',
	id: 'umb-dashboard-settings-welcome',
} as Meta;

export const AAAOverview: StoryFn<UmbSettingsWelcomeDashboardElement> = () =>
	html` <umb-settings-welcome-dashboard></umb-settings-welcome-dashboard>`;
AAAOverview.storyName = 'Overview';
