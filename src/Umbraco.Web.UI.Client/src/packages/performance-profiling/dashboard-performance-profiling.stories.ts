import type { UmbDashboardPerformanceProfilingElement } from './dashboard-performance-profiling.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './dashboard-performance-profiling.element.js';

export default {
	title: 'Dashboards/Performance Profiling',
	component: 'umb-dashboard-performance-profiling',
	id: 'umb-dashboard-performance-profiling',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardPerformanceProfilingElement> = () =>
	html` <umb-dashboard-performance-profiling></umb-dashboard-performance-profiling>`;
AAAOverview.storyName = 'Overview';
