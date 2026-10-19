import { UmbBlockGridManagerContext } from './block-grid-manager.context.js';
import type { UmbBlockGridLayoutModel } from '../types.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementDetailRepository } from '@umbraco-cms/backoffice/element';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-block-grid-manager-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockGridManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

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
