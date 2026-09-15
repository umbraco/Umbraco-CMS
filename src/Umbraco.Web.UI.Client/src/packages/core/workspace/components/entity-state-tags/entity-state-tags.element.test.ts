import { UmbEntityStateTagsElement } from './entity-state-tags.element.js';
import type { UmbEntityStateEntry, UmbEntityStateLook } from '@umbraco-cms/backoffice/entity-state';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbEntityStateTagsElement', () => {
	let element: UmbEntityStateTagsElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-entity-state-tags></umb-entity-state-tags>`);
	});

	function tags(): Array<Element> {
		return Array.from(element.shadowRoot!.querySelectorAll('uui-tag'));
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEntityStateTagsElement);
	});

	it('renders nothing for an empty array', () => {
		expect(tags()).to.have.lengthOf(0);
	});

	it('renders one tag per state, in the given array order', async () => {
		element.states = [
			{ unique: 'a', message: 'First' },
			{ unique: 'b', message: 'Second' },
		];
		await element.updateComplete;

		expect(tags()).to.have.lengthOf(2);
		expect(tags()[0].textContent?.trim()).to.equal('First');
		expect(tags()[1].textContent?.trim()).to.equal('Second');
	});

	const lookCases: Array<{ look: UmbEntityStateLook | undefined; expectedColor: string }> = [
		{ look: 'positive', expectedColor: 'positive' },
		{ look: 'warning', expectedColor: 'warning' },
		{ look: 'danger', expectedColor: 'danger' },
		{ look: 'neutral', expectedColor: 'default' },
		{ look: undefined, expectedColor: 'default' },
	];

	lookCases.forEach(({ look, expectedColor }) => {
		it(`maps look "${look}" to color "${expectedColor}"`, async () => {
			const state: UmbEntityStateEntry = { unique: 'a', message: 'Test', look };
			element.states = [state];
			await element.updateComplete;

			expect(tags()[0].getAttribute('color')).to.equal(expectedColor);
		});
	});

	it('always sets look="secondary" on the rendered tag', async () => {
		element.states = [{ unique: 'a', message: 'Test' }];
		await element.updateComplete;

		expect(tags()[0].getAttribute('look')).to.equal('secondary');
	});

	it('sets title to the localized detail when present', async () => {
		element.states = [{ unique: 'a', message: 'Test', detail: 'Some detail' }];
		await element.updateComplete;

		expect(tags()[0].getAttribute('title')).to.equal('Some detail');
	});

	it('omits the title attribute entirely when detail is absent', async () => {
		element.states = [{ unique: 'a', message: 'Test' }];
		await element.updateComplete;

		expect(tags()[0].hasAttribute('title')).to.be.false;
	});
});
