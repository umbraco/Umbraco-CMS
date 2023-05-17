import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDashboardTelemetryElement } from './dashboard-telemetry.element';
import './dashboard-telemetry.element';

export default {
	title: 'Dashboards/Telemetry',
	component: 'umb-dashboard-telemetry',
	id: 'umb-dashboard-telemetry',
} as Meta;

export const AAAOverview: Story<UmbDashboardTelemetryElement> = () =>
	html` <umb-dashboard-telemetry></umb-dashboard-telemetry>`;
AAAOverview.storyName = 'Overview';
