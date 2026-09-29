import type { UmbLanguageItemModel } from '../types.js';
import { UmbLanguageItemRefElement } from './langauge-item-ref.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbLanguageItemRefElement', () => {
	let element: UmbLanguageItemRefElement;

	const item = { unique: 'en-US', name: 'English (United States)' } as UmbLanguageItemModel;

	beforeEach(async () => {
		element = await fixture<UmbLanguageItemRefElement>(html`<umb-language-item-ref></umb-language-item-ref>`);
		element.item = item;
		await element.updateComplete;
	});

	const getFrame = () => element.shadowRoot!.querySelector('umb-entity-frame');

	it('renders the entity frame with the item name when editable', () => {
		expect(getFrame()?.textContent).to.contain(item.name);
	});

	it('renders no entity frame when readonly', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(getFrame()).to.equal(null);
	});

	it('renders no entity frame when the item has no unique', async () => {
		element.item = { ...item, unique: '' };
		await element.updateComplete;
		expect(getFrame()).to.equal(null);
	});
});
