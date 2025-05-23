import type { UmbDashboardTelemetryElement } from './dashboard-telemetry.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './dashboard-telemetry.element.js';

export default {
	title: 'Dashboards/Telemetry',
	component: 'umb-dashboard-telemetry',
	id: 'umb-dashboard-telemetry',
} as Meta;

export const AAAOverview: StoryFn<UmbDashboardTelemetryElement> = () =>
	html` <umb-dashboard-telemetry></umb-dashboard-telemetry>`;
AAAOverview.storyName = 'Overview';
