import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardModelsBuilderElement } from './dashboard-models-builder.element';
import './dashboard-models-builder.element';

export default {
	title: 'Dashboards/Models Builder',
	component: 'umb-dashboard-models-builder',
	id: 'umb-dashboard-models-builder',
} as Meta;

export const AAAOverview: Story<UmbDashboardModelsBuilderElement> = () =>
	html` <umb-dashboard-models-builder></umb-dashboard-models-builder>`;
AAAOverview.storyName = 'Overview';
