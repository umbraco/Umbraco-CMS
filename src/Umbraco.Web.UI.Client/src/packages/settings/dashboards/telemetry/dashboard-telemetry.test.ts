import { expect, fixture, html } from '@open-wc/testing';
import { UmbDashboardTelemetryElement } from './dashboard-telemetry.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbDashboardTelemetryElement', () => {
	let element: UmbDashboardTelemetryElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-dashboard-telemetry></umb-dashboard-telemetry>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDashboardTelemetryElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
