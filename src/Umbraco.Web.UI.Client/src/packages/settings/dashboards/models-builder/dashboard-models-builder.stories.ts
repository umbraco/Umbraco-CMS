import { Meta, Story } from '@storybook/web-components';
import type { UmbDashboardModelsBuilderElement } from './dashboard-models-builder.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './dashboard-models-builder.element.js';

export default {
	title: 'Dashboards/Models Builder',
	component: 'umb-dashboard-models-builder',
	id: 'umb-dashboard-models-builder',
} as Meta;

export const AAAOverview: Story<UmbDashboardModelsBuilderElement> = () =>
	html` <umb-dashboard-models-builder></umb-dashboard-models-builder>`;
AAAOverview.storyName = 'Overview';
