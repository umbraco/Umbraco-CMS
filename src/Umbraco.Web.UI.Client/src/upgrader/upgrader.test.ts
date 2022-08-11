import { expect, fixture, html } from '@open-wc/testing';

import { UmbUpgrader } from './upgrader.element';

describe('UmbUpgrader', () => {
	let element: UmbUpgrader;

	beforeEach(async () => {
		element = await fixture(html`<umb-upgrader></umb-upgrader>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbUpgrader);
	});
});
