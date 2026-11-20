import { UmbUserInputElement } from './user-input.element.js';
import { UMB_USER_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { provideTestModalManager, registerTestItemRepository } from '@umbraco-cms/internal/test-utils';

describe('UmbUserInputElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-user-input></umb-user-input>`);
		expect(element).to.be.instanceOf(UmbUserInputElement);
	});

	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_USER_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'user',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-user-input');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbUserInputElement) =>
			input.shadowRoot!.querySelector<HTMLElement>(
				`uui-action-bar uui-button[label="${input.localize.term('actions_replace')}"]`,
			);

		it('offers replace when a single item is allowed', async () => {
			const { input } = await setup(1);
			expect(replaceButton(input)).to.exist;
		});

		it('does not offer replace when more than one item is allowed', async () => {
			const { input } = await setup(2);
			expect(replaceButton(input)).to.be.null;
		});
	});
});
