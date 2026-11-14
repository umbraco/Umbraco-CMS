import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import { manifests as itemRepositoryManifests } from '../../item/repository/manifests.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberPickerModalData } from '../member-picker-modal/member-picker-modal.token.js';
import { UmbMemberPickerInputContext } from './input-member.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_MEMBER_TYPE_ENTITY_TYPE, type UmbMemberTypeEntityType } from '@umbraco-cms/backoffice/member-type';

@customElement('umb-test-input-member-context-host')
class UmbTestInputMemberContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const member = (memberTypeUnique: string, name = 'Member'): UmbMemberDetailModel =>
	({
		unique: name,
		memberType: { unique: memberTypeUnique },
		variants: [{ name, culture: null }],
	}) as UmbMemberDetailModel;

const allowedContentTypes: Array<{ unique: string; entityType: UmbMemberTypeEntityType }> = [
	{ unique: 'allowed-type', entityType: UMB_MEMBER_TYPE_ENTITY_TYPE },
];

describe('UmbMemberPickerInputContext', () => {
	let context: UmbMemberPickerInputContext;
	let openedWith: Array<UmbMemberPickerModalData>;

	before(() => {
		umbExtensionsRegistry.registerMany(itemRepositoryManifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(itemRepositoryManifests.map((manifest) => manifest.alias));
	});

	beforeEach(async () => {
		const host = await fixture<UmbTestInputMemberContextHostElement>(
			html`<umb-test-input-member-context-host></umb-test-input-member-context-host>`,
		);

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

		context = new UmbMemberPickerInputContext(host);
	});

	describe('pickable filter', () => {
		it('only lets members of the allowed member types be picked', async () => {
			await context.openPicker({}, { allowedContentTypes });

			const pickableFilter = openedWith[0].pickableFilter!;
			expect(pickableFilter(member('allowed-type'))).to.be.true;
			expect(pickableFilter(member('other-type'))).to.be.false;
		});

		it('requires both the allowed member types and the given pickable filter to accept a member', async () => {
			await context.openPicker(
				{ pickableFilter: (item) => item.variants[0].name !== 'Rejected' },
				{ allowedContentTypes },
			);

			const pickableFilter = openedWith[0].pickableFilter!;
			expect(pickableFilter(member('allowed-type', 'Accepted'))).to.be.true;
			expect(pickableFilter(member('allowed-type', 'Rejected'))).to.be.false;
			expect(pickableFilter(member('other-type', 'Accepted'))).to.be.false;
		});
	});

	describe('search', () => {
		it('uses the member search provider by default', async () => {
			await context.openPicker();

			expect(openedWith[0].search?.providerAlias).to.equal(UMB_MEMBER_SEARCH_PROVIDER_ALIAS);
		});

		it('uses the member search provider when only a search pickable filter is given', async () => {
			const pickableFilter = () => true;

			await context.openPicker({ search: { pickableFilter } });

			expect(openedWith[0].search?.providerAlias).to.equal(UMB_MEMBER_SEARCH_PROVIDER_ALIAS);
			expect(openedWith[0].search?.pickableFilter).to.equal(pickableFilter);
		});

		it('uses the given search provider', async () => {
			await context.openPicker({ search: { providerAlias: 'My.SearchProvider' } });

			expect(openedWith[0].search?.providerAlias).to.equal('My.SearchProvider');
		});

		it('passes the allowed member types to the search query alongside the given query params', async () => {
			await context.openPicker({ search: { queryParams: { culture: 'en-US' } } }, { allowedContentTypes });

			expect(openedWith[0].search?.queryParams).to.deep.equal({ allowedContentTypes, culture: 'en-US' });
		});
	});
});
