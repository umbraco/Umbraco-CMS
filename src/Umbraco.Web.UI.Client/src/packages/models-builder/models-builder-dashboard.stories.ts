import type { UmbModelsBuilderDashboardElement } from './models-builder-dashboard.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './models-builder-dashboard.element.js';

export default {
	title: 'Dashboards/Models Builder',
	component: 'umb-models-builder-dashboard',
	id: 'umb-models-builder-dashboard',
} as Meta;

export const AAAOverview: StoryFn<UmbModelsBuilderDashboardElement> = () =>
	html` <umb-models-builder-dashboard></umb-models-builder-dashboard>`;
AAAOverview.storyName = 'Overview';
