import { manifests as itemRepositoryManifests } from '../../repository/item/manifests.js';
import type { UmbLanguageItemModel } from '../../types.js';
import type { UmbLanguagePickerModalData } from '../../modals/language-picker/language-picker-modal.token.js';
import { UmbInputLanguageElement } from './input-language.element.js';
import { UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { provideTestModalManager, registerTestItemRepository } from '@umbraco-cms/internal/test-utils';

const language = (unique: string) => ({ unique, name: unique }) as UmbLanguageItemModel;

describe('UmbInputLanguageElement', () => {
	let element: UmbInputLanguageElement;
	let openedWith: Array<UmbLanguagePickerModalData>;

	const openPicker = async () => {
		element.shadowRoot!.querySelector<HTMLElement>('#btn-add')!.click();
		await waitUntil(() => openedWith.length === 1, 'the picker modal was opened');
		return openedWith[0];
	};

	before(() => {
		umbExtensionsRegistry.registerMany(itemRepositoryManifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(itemRepositoryManifests.map((manifest) => manifest.alias));
	});

	beforeEach(async () => {
		const host = await fixture<HTMLElement>(html`<div><umb-input-language></umb-input-language></div>`);
		element = host.querySelector<UmbInputLanguageElement>('umb-input-language')!;

		openedWith = [];
		const modalManager = {
			getHostElement: () => host,
			open: (_host: unknown, _token: unknown, args: { data: UmbLanguagePickerModalData }) => {
				openedWith.push(args.data);
				return { onSubmit: () => Promise.reject(new Error('Cancelled')) };
			},
		};
		new UmbContextProvider(
			host,
			UMB_MODAL_MANAGER_CONTEXT,
			modalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
		).hostConnected();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputLanguageElement);
	});

	describe('pickable filters', () => {
		it('prevents languages rejected by the browse pickable filter from being picked', async () => {
			element.browsePickableFilter = (item) => item.unique !== 'da';

			const { pickableFilter } = await openPicker();

			expect(pickableFilter!(language('en-US'))).to.be.true;
			expect(pickableFilter!(language('da'))).to.be.false;
		});

		it('passes the deprecated filter to the picker', async () => {
			const filter = () => true;
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			element.filter = filter;

			// eslint-disable-next-line @typescript-eslint/no-deprecated
			expect((await openPicker()).filter).to.equal(filter);
		});

		it('does not pass a filter to the picker when none is set', async () => {
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			expect((await openPicker()).filter).to.be.undefined;
		});
	});
});

describe('UmbInputLanguageElement', () => {
	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'language',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-language');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputLanguageElement) =>
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
