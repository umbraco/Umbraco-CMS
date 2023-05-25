import { Meta, Story } from '@storybook/web-components';
import type { UmbDashboardSettingsWelcomeElement } from './dashboard-settings-welcome.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './dashboard-settings-welcome.element.js';

export default {
	title: 'Dashboards/Settings Welcome',
	component: 'umb-dashboard-settings-welcome',
	id: 'umb-dashboard-settings-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardSettingsWelcomeElement> = () =>
	html` <umb-dashboard-settings-welcome></umb-dashboard-settings-welcome>`;
AAAOverview.storyName = 'Overview';
