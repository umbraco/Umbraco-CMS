import './umbraco-news-dashboard.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbUmbracoNewsDashboardElement } from './umbraco-news-dashboard.element.js';

export default {
	title: 'Dashboards/Umbraco News',
	component: 'umb-umbraco-news-dashboard',
	id: 'umb-umbraco-news-dashboard',
} as Meta;

export const AAAOverview: Story<UmbUmbracoNewsDashboardElement> = () =>
	html` <umb-umbraco-news-dashboard></umb-umbraco-news-dashboard>`;
AAAOverview.storyName = 'Overview';
