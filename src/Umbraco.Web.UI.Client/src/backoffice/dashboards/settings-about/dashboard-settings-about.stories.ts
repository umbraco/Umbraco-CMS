import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbDashboardSettingsAboutElement } from './dashboard-settings-about.element';
import './dashboard-settings-about.element';

export default {
	title: 'Dashboards/Settings About',
	component: 'umb-dashboard-settings-about',
	id: 'umb-dashboard-settings-about',
} as Meta;

export const AAAOverview: Story<UmbDashboardSettingsAboutElement> = () =>
	html` <umb-dashboard-settings-about></umb-dashboard-settings-about>`;
AAAOverview.storyName = 'Overview';
