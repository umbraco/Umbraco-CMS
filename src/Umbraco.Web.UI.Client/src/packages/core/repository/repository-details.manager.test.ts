import { UmbRepositoryDetailsManager } from './repository-details.manager.js';
import type { UmbDetailRepository } from './detail/index.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-test-repository-details-manager-host')
class UmbTestRepositoryDetailsManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface UmbTestDetailModel {
	unique: string;
	name: string;
}

const makeEntry = (unique: string): UmbTestDetailModel => ({ unique, name: `Entry ${unique}` });

describe('UmbRepositoryDetailsManager', () => {
	let host: UmbTestRepositoryDetailsManagerHostElement;
	let manager: UmbRepositoryDetailsManager<UmbTestDetailModel>;
	let requestedIndividually: Array<string>;

	/**
	 * Only the reading half of the repository is exercised here, so the rest is left unimplemented rather than
	 * stubbed out with behaviour no test relies on.
	 */
	const createRepository = (
		requestByUniques: (uniques: Array<string>) => Promise<{ data?: Array<UmbTestDetailModel>; error?: unknown }>,
	) =>
		({
			requestByUniques,
			requestByUnique: async (unique: string) => {
				requestedIndividually.push(unique);
				return { data: makeEntry(unique) };
			},
		}) as unknown as UmbDetailRepository<UmbTestDetailModel>;

	beforeEach(() => {
		requestedIndividually = [];
		host = new UmbTestRepositoryDetailsManagerHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		manager?.destroy();
		document.body.innerHTML = '';
	});

	it('requests the entries in bulk when the bulk request succeeds', async () => {
		const repository = createRepository(async (uniques) => ({ data: uniques.map(makeEntry) }));
		manager = new UmbRepositoryDetailsManager<UmbTestDetailModel>(host, repository);

		manager.setUniques(['a', 'b', 'c']);
		await aTimeout();

		const entries = await firstValueFrom(manager.entries);
		expect(entries.map((entry) => entry.unique)).to.have.members(['a', 'b', 'c']);
		expect(requestedIndividually).to.be.empty;
	});

	it('requests the entries individually when the bulk request fails', async () => {
		// A bulk request can fail for reasons that have nothing to do with the uniques it carries, notably when its
		// query string pushes the request over the server's request size limit. The individual requests still work.
		const repository = createRepository(async () => ({ error: new Error('Request too long') }));
		manager = new UmbRepositoryDetailsManager<UmbTestDetailModel>(host, repository);

		manager.setUniques(['a', 'b', 'c']);
		await aTimeout();

		expect(requestedIndividually).to.have.members(['a', 'b', 'c']);

		const entries = await firstValueFrom(manager.entries);
		expect(entries.map((entry) => entry.unique)).to.have.members(['a', 'b', 'c']);
	});
});

function aTimeout(ms = 50) {
	return new Promise((resolve) => setTimeout(resolve, ms));
}
