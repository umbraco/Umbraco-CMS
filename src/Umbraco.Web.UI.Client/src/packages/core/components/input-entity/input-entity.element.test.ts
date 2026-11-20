import type { UmbInputEntityElement } from './input-entity.element.js';
import './input-entity.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { provideTestModalManager, registerTestItemRepository } from '@umbraco-cms/internal/test-utils';

const TEST_REPOSITORY_ALIAS = 'Umb.Test.Repository.InputEntityItem';

class UmbTestPickerInputContext extends UmbPickerInputContext {
	constructor(host: UmbControllerHost) {
		super(host, TEST_REPOSITORY_ALIAS, 'Umb.Test.Modal.InputEntity');
	}
}

describe('UmbInputEntityElement', () => {
	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(TEST_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'test',
				name: `Item ${unique}`,
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-entity');
			input.pickerContext = UmbTestPickerInputContext;
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputEntityElement) =>
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
