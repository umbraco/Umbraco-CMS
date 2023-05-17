import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardPerformanceProfilingElement } from './dashboard-performance-profiling.element';
import './dashboard-performance-profiling.element';

export default {
	title: 'Dashboards/Performance Profiling',
	component: 'umb-dashboard-performance-profiling',
	id: 'umb-dashboard-performance-profiling',
} as Meta;

export const AAAOverview: Story<UmbDashboardPerformanceProfilingElement> = () =>
	html` <umb-dashboard-performance-profiling></umb-dashboard-performance-profiling>`;
AAAOverview.storyName = 'Overview';
