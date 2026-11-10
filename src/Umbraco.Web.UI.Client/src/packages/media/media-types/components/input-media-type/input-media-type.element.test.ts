import { UmbInputMediaTypeElement } from './input-media-type.element.js';
import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { provideTestModalManager, registerTestItemRepository } from '@umbraco-cms/internal/test-utils';

describe('UmbInputMediaTypeElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-input-media-type></umb-input-media-type>`);
		expect(element).to.be.instanceOf(UmbInputMediaTypeElement);
	});

	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'media-type',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-media-type');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputMediaTypeElement) =>
			input.shadowRoot!.querySelector<HTMLElement>(
				`uui-action-bar uui-button[label="${input.localize.term('actions_replace')}"]`,
			);

		it('replaces the only item with the one picked, when a single item is allowed', async () => {
			const { input, opened } = await setup(1);
			expect(replaceButton(input), 'replace button').to.exist;

			replaceButton(input)!.click();
			await waitUntil(() => input.selection[0] === 'b', 'the item was not changed');

			expect(opened).to.have.lengthOf(1);
			expect(input.selection).to.deep.equal(['b']);
		});

		it('does not offer replace when more than one item is allowed', async () => {
			const { input } = await setup(2);
			expect(replaceButton(input)).to.be.null;
		});
	});
});
