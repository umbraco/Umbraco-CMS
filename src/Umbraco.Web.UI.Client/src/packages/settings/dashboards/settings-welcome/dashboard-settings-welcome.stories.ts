import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardSettingsWelcomeElement } from './dashboard-settings-welcome.element';
import './dashboard-settings-welcome.element';

export default {
	title: 'Dashboards/Settings Welcome',
	component: 'umb-dashboard-settings-welcome',
	id: 'umb-dashboard-settings-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardSettingsWelcomeElement> = () =>
	html` <umb-dashboard-settings-welcome></umb-dashboard-settings-welcome>`;
AAAOverview.storyName = 'Overview';
