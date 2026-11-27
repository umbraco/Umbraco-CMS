import { manifests as itemRepositoryManifests } from '../../repository/item/manifests.js';
import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import type { UmbStaticFilePickerModalData } from '../../modals/index.js';
import { UmbInputStaticFileElement } from './input-static-file.element.js';
import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import {
	type UmbTestRunnerWindow,
	defaultA11yConfig,
	provideTestModalManager,
	registerTestItemRepository,
} from '@umbraco-cms/internal/test-utils';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

const staticFile = (name: string) => ({ unique: name, name }) as UmbStaticFileItemModel;

describe('UmbInputStaticFileElement', () => {
	let element: UmbInputStaticFileElement;
	let openedWith: Array<UmbStaticFilePickerModalData>;

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
		const host = await fixture<HTMLElement>(html`<div><umb-input-static-file></umb-input-static-file></div>`);
		element = host.querySelector<UmbInputStaticFileElement>('umb-input-static-file')!;

		openedWith = [];
		const modalManager = {
			getHostElement: () => host,
			open: (_host: unknown, _token: unknown, args: { data: UmbStaticFilePickerModalData }) => {
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
		expect(element).to.be.instanceOf(UmbInputStaticFileElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('Public API', () => {
		describe('properties', () => {
			it('has a selection property', () => {
				expect(element).to.have.property('selection').to.be.an.instanceOf(Array);
			});

			it('has a value property', () => {
				expect(element).to.have.property('value');
			});
		});
	});

	describe('pickable filters', () => {
		it('prevents files rejected by the browse pickable filter from being picked', async () => {
			element.browsePickableFilter = (item) => item.name !== 'rejected.js';

			const { pickableFilter } = await openPicker();

			expect(pickableFilter!(staticFile('accepted.js'))).to.be.true;
			expect(pickableFilter!(staticFile('rejected.js'))).to.be.false;
		});

		it('applies the deprecated pickable filter as the browse pickable filter', async () => {
			const filter = (item: UmbStaticFileItemModel) => item.name !== 'rejected.js';
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			element.pickableFilter = filter;

			expect(element.browsePickableFilter).to.equal(filter);
			expect((await openPicker()).pickableFilter).to.equal(filter);
		});
	});
});

describe('UmbInputStaticFileElement', () => {
	describe('replace action', () => {
		let unregisterRepository: () => void;

		before(() => {
			unregisterRepository = registerTestItemRepository(UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, (unique) => ({
				unique,
				entityType: 'static-file',
				name: `Item ${unique}`,
				icon: 'icon-document',
			}));
		});

		after(() => unregisterRepository());

		const setup = async (max: number) => {
			const host = await fixture<HTMLElement>(html`<div></div>`);
			const opened = provideTestModalManager(host, { selection: ['b'] });
			const input = document.createElement('umb-input-static-file');
			input.max = max;
			input.selection = ['a'];
			host.appendChild(input);
			await waitUntil(() => input.shadowRoot?.querySelector('uui-action-bar'), 'the picked item was not rendered');
			return { input, opened };
		};

		const replaceButton = (input: UmbInputStaticFileElement) =>
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
