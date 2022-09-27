import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../../../core/helpers/chai';
import { UmbDashboardPublishedStatusElement } from './dashboard-published-status.element';

describe('UmbDashboardPublishedStatus', () => {
	let element: UmbDashboardPublishedStatusElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-dashboard-published-status></umb-dashboard-published-status>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDashboardPublishedStatusElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
