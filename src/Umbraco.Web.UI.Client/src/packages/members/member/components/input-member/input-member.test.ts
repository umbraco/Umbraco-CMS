import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import { manifests as itemRepositoryManifests } from '../../item/repository/manifests.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberPickerModalData } from '../member-picker-modal/member-picker-modal.token.js';
import { UmbInputMemberElement } from './input-member.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

const browseRow = (name: string): UmbMemberDetailModel =>
	({
		unique: name,
		memberType: { unique: 'member-type' },
		variants: [{ name, culture: null }],
	}) as UmbMemberDetailModel;

describe('UmbInputMemberElement', () => {
	let element: UmbInputMemberElement;
	let openedWith: Array<UmbMemberPickerModalData>;

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
		const host = await fixture<HTMLElement>(html`<div><umb-input-member></umb-input-member></div>`);
		element = host.querySelector<UmbInputMemberElement>('umb-input-member')!;

		openedWith = [];
		const modalManager = {
			getHostElement: () => host,
			open: (_host: unknown, _token: unknown, args: { data: UmbMemberPickerModalData }) => {
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
		expect(element).to.be.instanceOf(UmbInputMemberElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('pickable filters', () => {
		it('prevents members rejected by the browse pickable filter from being picked', async () => {
			element.browsePickableFilter = (item) => item.variants[0].name !== 'Rejected';

			const { pickableFilter } = await openPicker();

			expect(pickableFilter!(browseRow('Accepted'))).to.be.true;
			expect(pickableFilter!(browseRow('Rejected'))).to.be.false;
		});

		it('passes the search pickable filter to the member search', async () => {
			const searchPickableFilter = () => true;
			element.searchPickableFilter = searchPickableFilter;

			const { search } = await openPicker();

			expect(search?.providerAlias).to.equal(UMB_MEMBER_SEARCH_PROVIDER_ALIAS);
			expect(search?.pickableFilter).to.equal(searchPickableFilter);
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
