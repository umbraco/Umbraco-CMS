import type { UmbContentRollbackModalElement } from './content-rollback-modal.element.js';
import type { UmbContentRollbackRepository } from '../rollback-repository.interface.js';
import type { UmbContentRollbackVersionItemModel } from '../types.js';
import './content-rollback-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import { UMB_USER_ITEM_STORE_CONTEXT } from '@umbraco-cms/backoffice/user';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UUIPaginationElement, UUISelectElement } from '@umbraco-cms/backoffice/external/uui';

const ROLLBACK_REPOSITORY_ALIAS = 'Umb.Test.Repository.Rollback';
const DETAIL_REPOSITORY_ALIAS = 'Umb.Test.Repository.Detail';
const PAGE_SIZE = 20;
const VERSION_COUNT = 45;

type VersionRequest = { culture?: string; skip?: number; take?: number };

// Newest first, as the contract requires: the current draft, then the current published version.
const versions: Array<UmbContentRollbackVersionItemModel> = Array.from({ length: VERSION_COUNT }, (_, index) => ({
	id: `version-${index}`,
	versionDate: new Date(Date.UTC(2024, 2, 5, 10, 0) - index * 60_000).toISOString(),
	user: { id: 'unknown-user' },
	isCurrentDraftVersion: index === 0,
	isCurrentPublishedVersion: index === 1,
	preventCleanup: false,
}));

let requests: Array<VersionRequest> = [];
let cultures: Array<string | null> = [null];

class UmbTestRollbackRepository implements UmbContentRollbackRepository {
	async requestVersionsByEntityId(_id: string, culture?: string, skip = 0, take = 100) {
		requests.push({ culture, skip, take });
		return { data: { items: versions.slice(skip, skip + take), total: versions.length } };
	}

	async requestVersionById(id: string) {
		return { data: { id, variants: cultures.map((culture) => ({ culture, name: 'Page' })), values: [] } };
	}

	async setPreventCleanup() {
		return {};
	}

	async rollback() {
		return {};
	}

	destroy() {}
}

class UmbTestDetailRepository {
	async requestByUnique(unique: string) {
		return {
			data: {
				unique,
				entityType: 'document',
				variants: cultures.map((culture) => ({ culture, segment: null, name: 'Page' })),
				values: [],
			},
		};
	}

	destroy() {}
}

class UmbTestItemStore extends UmbItemStoreBase<{ unique: string }> {}

@customElement('umb-test-content-rollback-host')
class UmbTestContentRollbackHostElement extends UmbElementMixin(HTMLElement) {
	constructor() {
		super();
		const entityContext = new UmbEntityContext(this);
		entityContext.setEntityType('document');
		entityContext.setUnique('page');
		new UmbTestItemStore(this as UmbControllerHost, UMB_USER_ITEM_STORE_CONTEXT);
		// The language item store token is internal to the language package; tokens resolve by alias.
		new UmbTestItemStore(this as UmbControllerHost, 'UmbLanguageItemStore');
	}
}

describe('UmbContentRollbackModalElement', () => {
	let host: UmbTestContentRollbackHostElement;
	let element: UmbContentRollbackModalElement;
	let restoreErrorHandler: () => void;

	const rows = () => Array.from(element.shadowRoot!.querySelectorAll<HTMLElement>('.rollback-item'));
	const rowDates = () => rows().map((row) => row.querySelector('umb-localize-date')!.getAttribute('date'));
	const selectedRow = () => element.shadowRoot!.querySelector<HTMLElement>('.rollback-item.active');
	const pagination = () => element.shadowRoot!.querySelector<UUIPaginationElement>('uui-pagination')!;

	const render = async () => {
		host = document.createElement('umb-test-content-rollback-host') as UmbTestContentRollbackHostElement;
		element = document.createElement('umb-content-rollback-modal');
		element.data = {
			rollbackRepositoryAlias: ROLLBACK_REPOSITORY_ALIAS,
			detailRepositoryAlias: DETAIL_REPOSITORY_ALIAS,
		};
		host.appendChild(element);
		document.body.appendChild(host);
		await waitUntil(() => rows().length > 0, 'the versions were never rendered');
		await waitUntil(() => selectedRow() !== null, 'no version was selected');
	};

	const goToPage = async (page: number) => {
		const before = rowDates().join();
		const control = pagination();
		control.current = page;
		control.dispatchEvent(new CustomEvent('change'));
		await waitUntil(() => rowDates().join() !== before, 'the versions were never replaced');
	};

	beforeEach(() => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();
		requests = [];
		cultures = [null];
		umbExtensionsRegistry.register({
			type: 'repository',
			alias: ROLLBACK_REPOSITORY_ALIAS,
			name: 'Test Rollback Repository',
			api: UmbTestRollbackRepository,
		});
		umbExtensionsRegistry.register({
			type: 'repository',
			alias: DETAIL_REPOSITORY_ALIAS,
			name: 'Test Detail Repository',
			api: UmbTestDetailRepository,
		});
	});

	afterEach(() => {
		restoreErrorHandler();
		host.remove();
		umbExtensionsRegistry.unregister(ROLLBACK_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(DETAIL_REPOSITORY_ALIAS);
	});

	it('lists the current draft version as a row that cannot be selected', async () => {
		await render();

		const draft = rows()[0];
		expect(draft.classList.contains('draft')).to.be.true;
		expect(draft.textContent).to.contain(element.localize.term('rollback_currentDraftVersion'));
		expect(draft.querySelector('uui-button')).to.not.exist;

		draft.click();
		await element.updateComplete;
		expect(draft.classList.contains('active')).to.be.false;
	});

	it('offers to prevent clean up of every version but the current draft', async () => {
		await render();

		expect(rows().filter((row) => row.querySelector('uui-button'))).to.have.length(PAGE_SIZE - 1);
	});

	it('selects the current published version by default', async () => {
		await render();

		expect(selectedRow()).to.equal(rows()[1]);
		expect(selectedRow()!.textContent).to.contain(element.localize.term('rollback_currentPublishedVersion'));
	});

	it('shows the year in the version dates', async () => {
		await render();

		const date = rows()[1].querySelector('umb-localize-date')!;
		expect(date.shadowRoot!.textContent).to.contain('2024');
	});

	it('keeps the selected version when the page is changed', async () => {
		await render();
		const selectedDate = selectedRow()!.querySelector('umb-localize-date')!.getAttribute('date');

		await goToPage(2);
		expect(selectedRow()).to.be.null;

		await goToPage(1);
		expect(selectedRow()!.querySelector('umb-localize-date')!.getAttribute('date')).to.equal(selectedDate);
	});

	it('returns to the first page when the language is changed', async () => {
		cultures = ['en-US', 'da'];
		await render();
		await goToPage(3);

		const languageSelect = () => element.shadowRoot!.querySelector<UUISelectElement>('#language-select');
		await waitUntil(() => languageSelect() !== null, 'the language select was never rendered');
		const select = languageSelect()!;
		const before = requests.length;
		select.value = 'da';
		select.dispatchEvent(new CustomEvent('change'));
		await waitUntil(() => requests.length > before, 'the versions were never requested again');

		expect(requests.at(-1)).to.include({ culture: 'da', skip: 0, take: PAGE_SIZE });
	});
});
