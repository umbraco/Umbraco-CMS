import { expect } from '@open-wc/testing';
import { UmbRepositoryDetailsManager } from './repository-details.manager.js';
import type { UmbDetailRepository } from './detail/detail-repository.interface.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

interface UmbTestDetailModel {
	unique: string;
	name: string;
}

@customElement('umb-test-repository-details-manager-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

function createRepository(store: UmbObjectState<UmbTestDetailModel | undefined>) {
	return {
		byUnique: async () => store.asObservable(),
		requestByUnique: async () => ({ data: store.getValue(), asObservable: () => store.asObservable() }),
	} as unknown as UmbDetailRepository<UmbTestDetailModel>;
}

async function settle() {
	await new Promise((resolve) => setTimeout(resolve));
}

describe('UmbRepositoryDetailsManager', () => {
	describe('addEntry', () => {
		it('follows store updates of an added entry', async () => {
			const store = new UmbObjectState<UmbTestDetailModel | undefined>({ unique: '1', name: 'Original' });
			const manager = new UmbRepositoryDetailsManager(new UmbTestHostElement(), createRepository(store));

			manager.addEntry({ unique: '1', name: 'Original' });
			await settle();
			store.setValue({ unique: '1', name: 'Updated' });
			await settle();

			expect(manager.getEntries()).to.deep.equal([{ unique: '1', name: 'Updated' }]);
		});

		it('keeps an added entry the store does not hold', async () => {
			const store = new UmbObjectState<UmbTestDetailModel | undefined>(undefined);
			const manager = new UmbRepositoryDetailsManager(new UmbTestHostElement(), createRepository(store));

			manager.addEntry({ unique: '1', name: 'Original' });
			await settle();

			expect(manager.getEntries()).to.deep.equal([{ unique: '1', name: 'Original' }]);
		});
	});
});
