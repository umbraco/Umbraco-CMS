import { expect, fixture, html } from '@open-wc/testing';

import { UmbDashboardPublishedStatusElement } from './dashboard-published-status.element';

describe('UmbDashboardPublishedStatus', () => {
	let element: UmbDashboardPublishedStatusElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-dashboard-published-status></umb-dashboard-published-status>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDashboardPublishedStatusElement);
	});
});
