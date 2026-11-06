import { UmbInputMemberElement } from './input-member.element.js';
import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS } from '../../item/repository/manifests.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import {
	type UmbTestRunnerWindow,
	defaultA11yConfig,
	provideTestModalManager,
	registerTestItemRepository,
} from '@umbraco-cms/internal/test-utils';
describe('UmbInputMemberElement', () => {
	let element: UmbInputMemberElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-member></umb-input-member> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMemberElement);
	});

	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_MEMBER_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'member',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-member');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputMemberElement) =>
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

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
