import { UmbBlockListManagerContext } from './block-list-manager.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementDetailRepository } from '@umbraco-cms/backoffice/element';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-block-list-manager-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockListManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockListManagerContext', () => {
	describe('insertExternalContent', () => {
		let host: UmbControllerHostElement;
		let manager: UmbBlockListManagerContext;
		const originalRequestByUniques = UmbElementDetailRepository.prototype.requestByUniques;

		const layout = (key: string) => ({ key, contentKey: `content-${key}` });
		const contentKeys = () => manager.getLayouts().map((x) => x.contentKey);

		beforeEach(async () => {
			UmbElementDetailRepository.prototype.requestByUniques = async () => ({ data: [] });
			host = await fixture(html`<umb-test-block-list-manager-host></umb-test-block-list-manager-host>`);
			manager = new UmbBlockListManagerContext(host);
			manager.setLayouts([layout('a'), layout('b')]);
		});

		afterEach(() => {
			UmbElementDetailRepository.prototype.requestByUniques = originalRequestByUniques;
			manager.destroy();
		});

		it('appends when no origin data is given', () => {
			manager.insertExternalContent('external');

			expect(contentKeys()).to.deep.equal(['content-a', 'content-b', 'external']);
		});

		it('appends when the index is -1', () => {
			manager.insertExternalContent('external', { index: -1 });

			expect(contentKeys()).to.deep.equal(['content-a', 'content-b', 'external']);
		});

		it('inserts at the index of the origin data', () => {
			manager.insertExternalContent('external', { index: 1 });

			expect(contentKeys()).to.deep.equal(['content-a', 'external', 'content-b']);
		});

		it('flags the inserted layout as external content', () => {
			manager.insertExternalContent('external', { index: 0 });

			expect(manager.getLayouts()[0].isExternalContent).to.be.true;
		});
	});
});
