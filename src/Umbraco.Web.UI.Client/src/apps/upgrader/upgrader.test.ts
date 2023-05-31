import { expect, fixture, html } from '@open-wc/testing';

import { UmbUpgraderElement } from './upgrader.element.js';

describe('UmbUpgrader', () => {
	let element: UmbUpgraderElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-upgrader></umb-upgrader>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbUpgraderElement);
	});
});
