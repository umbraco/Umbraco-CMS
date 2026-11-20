import { UmbInputMediaElement } from './input-media.element.js';
import { UmbMediaItemStore } from '../../repository/item/media-item.store.js';
import { manifests as mediaItemManifests } from '../../repository/item/manifests.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

const FLIPPED_CAR_ID = 'f2f81a40-c989-4b6b-84e2-057cecd3adc1';
const NO_ACCESS_ID = 'f2f81a40-c989-4b6b-84e2-057cecd3grd4';

// Provides what the app provides at runtime: the item store the repository waits for, and the
// ambient cultures the picker passes on to search.
@customElement('umb-test-input-media-host')
class UmbTestInputMediaHostElement extends UmbElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMediaItemStore(this);
		const variantContext = new UmbVariantContext(this);
		variantContext.setCulture(null);
		variantContext.setFallbackCulture('en-US');
	}
}

describe('UmbInputMediaElement', () => {
	let element: UmbInputMediaElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-media></umb-input-media> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMediaElement);
	});

	describe('interactionMemories', () => {
		it('seeds the picker context from the incoming snapshot', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a', 'b']);
		});

		it('removes memories that are no longer present when the snapshot shrinks', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [{ unique: 'a' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a']);
		});

		it('clears all memories when the snapshot is emptied', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [];
			expect(element.interactionMemories).to.eql([]);
		});
	});

	describe('replace action', () => {
		let host: UmbTestInputMediaHostElement;
		let openedModals: Array<{ multiple?: boolean }>;

		before(() => {
			umbExtensionsRegistry.registerMany(
				mediaItemManifests.filter((m) => !umbExtensionsRegistry.isRegistered(m.alias)),
			);
		});

		const setup = async (max: number) => {
			host = await fixture(html`<umb-test-input-media-host></umb-test-input-media-host>`);

			openedModals = [];
			const modalManager = {
				getHostElement: () => host,
				open: (_host: unknown, _token: unknown, args: { data: { multiple?: boolean } }) => {
					openedModals.push(args.data);
					return { onSubmit: () => Promise.resolve({ selection: [NO_ACCESS_ID] }) };
				},
			};
			host.provideContext(UMB_MODAL_MANAGER_CONTEXT, modalManager as never);

			element = document.createElement('umb-input-media');
			element.max = max;
			element.value = FLIPPED_CAR_ID;
			host.appendChild(element);

			await waitUntil(
				() => element.shadowRoot?.querySelector(`uui-card-media[data-unique="${FLIPPED_CAR_ID}"]`),
				'picked media was not loaded',
			);
		};

		const replaceButton = () =>
			element.shadowRoot!.querySelector<HTMLElement>(
				`uui-card-media uui-action-bar uui-button[label="${element.localize.term('actions_replace')}"]`,
			);

		it('replaces the only item with the one picked, when a single item is allowed', async () => {
			await setup(1);
			expect(replaceButton(), 'replace button').to.exist;

			replaceButton()!.click();
			await waitUntil(() => element.value === NO_ACCESS_ID, 'media was not changed');

			expect(openedModals).to.have.lengthOf(1);
			expect(openedModals[0].multiple).to.be.false;
		});

		it('does not offer replace when more than one item is allowed', async () => {
			await setup(2);
			expect(replaceButton()).to.be.null;
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
