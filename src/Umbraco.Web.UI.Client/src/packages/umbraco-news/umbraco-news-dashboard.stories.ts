import './umbraco-news-dashboard.element.js';

import type { UmbUmbracoNewsDashboardElement } from './umbraco-news-dashboard.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Dashboards/Umbraco News',
	component: 'umb-umbraco-news-dashboard',
	id: 'umb-umbraco-news-dashboard',
} as Meta;

export const AAAOverview: StoryFn<UmbUmbracoNewsDashboardElement> = () =>
	html` <umb-umbraco-news-dashboard></umb-umbraco-news-dashboard>`;
AAAOverview.storyName = 'Overview';
