import type { UmbBlockGridLayoutModel } from '../types.js';
import { UmbBlockGridManagerContext } from './block-grid-manager.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbElementDetailRepository } from '@umbraco-cms/backoffice/element';
import { UmbElementDetailStore } from '../../../elements/repository/detail/element-detail.store.js';

const ELEMENT_UNIQUE = 'library-element-one-id';

@customElement('umb-test-block-grid-manager-host')
class UmbTestBlockGridManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	actionEventContext = new UmbActionEventContext(this);
	constructor() {
		super();
		new UmbElementDetailStore(this);
	}
}

class UmbTestBlockGridManagerContext extends UmbBlockGridManagerContext {}

const layoutInArea = (contentKey: string): UmbBlockGridLayoutModel => ({
	key: 'area-item-key',
	contentKey,
	isExternalContent: true,
	areas: [],
	columnSpan: 12,
	rowSpan: 1,
});

describe('UmbBlockGridManagerContext - external content (Library Elements)', () => {
	let host: UmbTestBlockGridManagerHostElement;
	let manager: UmbTestBlockGridManagerContext;

	before(async () => {
		await useMockSet('blocksReusableContent');
	});

	beforeEach(() => {
		host = new UmbTestBlockGridManagerHostElement();
		document.body.appendChild(host);
		manager = new UmbTestBlockGridManagerContext(host);
	});

	afterEach(() => {
		manager.destroy();
		document.body.innerHTML = '';
	});

	it('resolves a Library Element referenced from a nested area', async () => {
		manager.setLayouts([
			{
				key: 'parent-key',
				contentKey: 'parent-content-key',
				areas: [{ key: 'area-key', items: [layoutInArea(ELEMENT_UNIQUE)] }],
				columnSpan: 12,
				rowSpan: 1,
			},
		]);

		const content = await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== undefined)));
		expect(content!.key).to.equal(ELEMENT_UNIQUE);
	});
});

describe('UmbBlockGridManagerContext', () => {
	describe('insertExternalContent', () => {
		let host: UmbControllerHostElement;
		let manager: UmbBlockGridManagerContext;
		const originalRequestByUniques = UmbElementDetailRepository.prototype.requestByUniques;

		const layout = (key: string, areas: UmbBlockGridLayoutModel['areas'] = []): UmbBlockGridLayoutModel => ({
			key,
			contentKey: `content-${key}`,
			columnSpan: 12,
			rowSpan: 1,
			areas,
		});
		const rootContentKeys = () => manager.getLayouts().map((x) => x.contentKey);
		const areaContentKeys = (parentKey: string, areaKey: string) =>
			manager
				.getLayouts()
				.find((x) => x.key === parentKey)
				?.areas?.find((x) => x.key === areaKey)
				?.items.map((x) => x.contentKey);

		beforeEach(async () => {
			UmbElementDetailRepository.prototype.requestByUniques = async () => ({ data: [] });
			host = await fixture(html`<umb-test-block-grid-manager-host></umb-test-block-grid-manager-host>`);
			manager = new UmbBlockGridManagerContext(host);
			manager.setLayouts([
				layout('a'),
				layout('parent', [{ key: 'area-1', items: [layout('x'), layout('y')] }]),
				layout('b'),
			]);
		});

		afterEach(() => {
			UmbElementDetailRepository.prototype.requestByUniques = originalRequestByUniques;
			manager.destroy();
		});

		it('appends to the root when no origin data is given', () => {
			manager.insertExternalContent('external');

			expect(rootContentKeys()).to.deep.equal(['content-a', 'content-parent', 'content-b', 'external']);
		});

		it('inserts at the index of the origin data in the root', () => {
			manager.insertExternalContent('external', { index: 1, parentUnique: null });

			expect(rootContentKeys()).to.deep.equal(['content-a', 'external', 'content-parent', 'content-b']);
		});

		it('inserts into the area of the origin data, at its index', () => {
			manager.insertExternalContent('external', { index: 1, parentUnique: 'parent', areaKey: 'area-1' });

			expect(areaContentKeys('parent', 'area-1')).to.deep.equal(['content-x', 'external', 'content-y']);
			expect(rootContentKeys()).to.deep.equal(['content-a', 'content-parent', 'content-b']);
		});

		it('appends to the area when the index is -1', () => {
			manager.insertExternalContent('external', { index: -1, parentUnique: 'parent', areaKey: 'area-1' });

			expect(areaContentKeys('parent', 'area-1')).to.deep.equal(['content-x', 'content-y', 'external']);
		});
	});
});
