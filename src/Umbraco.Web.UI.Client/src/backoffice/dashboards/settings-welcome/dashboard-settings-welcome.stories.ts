import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardSettingsWelcomeElement } from './dashboard-settings-welcome.element';
import './dashboard-settings-welcome.element';

export default {
	title: 'Dashboards/Settings Welcome',
	component: 'umb-dashboard-settings-welcome',
	id: 'umb-dashboard-settings-welcome',
} as Meta;

export const AAAOverview: Story<UmbDashboardSettingsWelcomeElement> = () =>
	html` <umb-dashboard-settings-welsome></umb-dashboard-settings-welsome>`;
AAAOverview.storyName = 'Overview';
