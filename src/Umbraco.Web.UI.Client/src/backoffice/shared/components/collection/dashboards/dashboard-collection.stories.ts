import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardCollectionElement } from './dashboard-collection.element';
import './dashboard-collection.element';

export default {
	title: 'Dashboards/Media Management',
	component: 'umb-dashboard-collection',
	id: 'umb-dashboard-collection',
} as Meta;

export const AAAOverview: Story<UmbDashboardCollectionElement> = () =>
	html` <umb-dashboard-collection></umb-dashboard-collection>`;
AAAOverview.storyName = 'Overview';
