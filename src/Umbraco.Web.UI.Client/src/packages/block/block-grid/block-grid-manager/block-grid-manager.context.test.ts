import type { UmbBlockGridLayoutModel } from '../types.js';
import { UmbBlockGridManagerContext } from './block-grid-manager.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
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
