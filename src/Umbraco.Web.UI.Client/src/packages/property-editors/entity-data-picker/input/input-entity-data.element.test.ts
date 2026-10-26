import { UmbInputEntityDataElement } from './input-entity-data.element.js';
import { UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS } from '../picker-item/constants.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import {
	type UmbTestRunnerWindow,
	defaultA11yConfig,
	provideTestModalManager,
	registerTestItemRepository,
} from '@umbraco-cms/internal/test-utils';

describe('UmbInputEntityDataElement', () => {
	let element: UmbInputEntityDataElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-entity-data></umb-input-entity-data> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputEntityDataElement);
	});

	describe('interactionMemories', () => {
		it('seeds the picker context from the incoming snapshot', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a', 'b']);
		});

		it('removes memories that are no longer present when the snapshot shrinks', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [{ unique: 'a' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a']);
		});

		it('clears all memories when the snapshot is emptied', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [];
			expect(element.interactionMemories).to.eql([]);
		});
	});

	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'entity-data',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-entity-data');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputEntityDataElement) =>
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

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
