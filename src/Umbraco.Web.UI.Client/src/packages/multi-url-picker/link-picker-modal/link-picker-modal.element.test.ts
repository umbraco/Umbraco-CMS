import type { UmbLinkPickerModalData, UmbLinkPickerModalValue } from './link-picker-modal.token.js';
import type { UmbLinkPickerLink } from './types.js';
import { UmbLinkPickerModalElement } from './link-picker-modal.element.js';
import { UMB_DOCUMENT_LINK_PICKER_MODAL } from '../document-link-picker-modal/document-link-picker-modal.token.js';
import { aTimeout, expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbPickerContext, UmbPickerModalBaseElement } from '@umbraco-cms/backoffice/picker';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityItemRefElement } from '@umbraco-cms/backoffice/entity-item';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type { UmbInputMultiUrlElement } from '../components/input-multi-url/index.js';
import { UmbDocumentItemStore } from '../../documents/documents/item/repository/document-item.store.js';
import { UmbDocumentUrlStore } from '../../documents/documents/url/repository/document-url.store.js';
import { UmbMediaItemStore } from '../../media/media/repository/item/media-item.store.js';
import { UmbMediaUrlStore } from '../../media/media/url/repository/media-url.store.js';
import { manifests as mediaItemManifests } from '../../media/media/repository/item/manifests.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import '../components/input-multi-url/index.js';

// Kitchen-sink documents and media.
const HOME_ID = 'db79156b-3d5b-43d6-ab32-902dc423bec3';
const CHECKBOX_LIST_ID = '9dfcda46-88bf-4d69-bb2d-e94667051727';
const IMAGE_ID = '76c02ec8-6a82-4c47-95da-56f6628b58fb';
const OTHER_IMAGE_ID = 'ee9c1205-4121-4610-b0b3-a522dfd3461d';

// Provides what the app provides at runtime: the item stores the repositories wait for, and the
// ambient cultures a document name resolves against.
@customElement('umb-test-link-picker-modal-host')
class UmbTestLinkPickerModalHostElement extends UmbElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentItemStore(this);
		new UmbDocumentUrlStore(this);
		new UmbMediaItemStore(this);
		new UmbMediaUrlStore(this);
		const variantContext = new UmbVariantContext(this);
		variantContext.setCulture(null);
		variantContext.setFallbackCulture('en-US');
	}
}

// Stands in for the document link picker modal, which this modal opens. Real modals live in the
// modal portal; nesting gives the same context resolution the modal context proxy arranges.
@customElement('test-nested-picker-modal')
class UmbTestNestedPickerModalElement extends UmbPickerModalBaseElement {
	protected override _pickerContext = new UmbPickerContext(this);
	public get pickerContext() {
		return this._pickerContext;
	}
}

const NESTED_MANIFEST: ManifestModal = {
	type: 'modal',
	alias: 'Umb.MultiUrlLinkPicker.Document',
	name: 'Test Document Link Picker Modal',
};

describe('UmbLinkPickerModalElement', () => {
	// The element only reads `value` and `size` off its modal context, and `UmbModalContext` is a
	// type-only export, so a stand-in covers what the memory relay needs.
	const createModalContext = () =>
		({
			value: new UmbObjectState<UmbLinkPickerModalValue>({ link: {} }).asObservable(),
			size: new UmbStringState('small').asObservable(),
		}) as unknown as UmbModalContext<UmbLinkPickerModalData, UmbLinkPickerModalValue>;

	// `<umb-input-multi-url>` is the element that provides the memory scope the modal writes to. At
	// runtime the modal reaches it through the modal context proxy rather than the DOM; nesting it
	// here gives the same context resolution.
	const setup = async () => {
		const input = await fixture<UmbInputMultiUrlElement>(html`<umb-input-multi-url></umb-input-multi-url>`);

		const modal = document.createElement('umb-link-picker-modal');
		modal.modalContext = createModalContext();
		input.appendChild(modal);
		await modal.updateComplete;

		const mediaInput = modal.shadowRoot!.querySelector<UmbInputMediaElement>('umb-input-media')!;
		await mediaInput.updateComplete;
		const mediaScope = (await mediaInput.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))!.memory;

		return { input, modal, mediaInput, mediaScope };
	};

	// What the media picker modal itself stores, keyed the way `UmbPickerModalBaseElement` keys it.
	const mediaPickerMemory = {
		unique: 'UmbPickerModal:Umb.Modal.MediaPicker',
		memories: [{ unique: 'UmbMediaPickerLocation', value: { unique: 'folder-1' } }],
	};

	const nested = [
		{
			unique: 'UmbLinkPickerModal',
			memories: [{ unique: 'UmbLinkPickerMedia', memories: [mediaPickerMemory] }],
		},
	];

	it('is defined with its own instance', async () => {
		const { modal } = await setup();
		expect(modal).to.be.instanceOf(UmbLinkPickerModalElement);
	});

	describe('interaction memory', () => {
		it('collects the media input memories under its own key and publishes one entry to the scope', async () => {
			const { input, mediaScope } = await setup();

			mediaScope.setMemory(mediaPickerMemory);
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(input.interactionMemories).to.deep.equal(nested);
		});

		it('seeds the media input from memories already held by the scope', async () => {
			const { input, mediaInput } = await setup();

			input.interactionMemories = nested;
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(mediaInput.interactionMemories).to.deep.equal([mediaPickerMemory]);
		});

		it('keeps what it holds when its entry disappears from the scope', async () => {
			const { input, mediaInput, mediaScope } = await setup();

			mediaScope.setMemory(mediaPickerMemory);
			await new Promise((resolve) => setTimeout(resolve, 0));

			// Simulates the scope being emptied from above (e.g. the app-root store dropping the
			// property editor's entry) while this modal is still open.
			input.interactionMemories = [];
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(mediaInput.interactionMemories).to.deep.equal([mediaPickerMemory]);
		});

		it('nests memories from a modal it opened inside its own entry', async () => {
			const { input, modal } = await setup();

			const nested = new UmbTestNestedPickerModalElement();
			nested.manifest = NESTED_MANIFEST;
			modal.appendChild(nested);
			await new Promise((resolve) => setTimeout(resolve, 0));

			nested.pickerContext.interactionMemory.setMemory({ unique: 'UmbTreeItemPickerExpansion', value: { a: 1 } });
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(input.interactionMemories).to.deep.equal([
				{
					unique: 'UmbLinkPickerModal',
					memories: [
						{
							unique: 'UmbPickerModal:Umb.MultiUrlLinkPicker.Document',
							memories: [{ unique: 'UmbTreeItemPickerExpansion', value: { a: 1 } }],
						},
					],
				},
			]);
		});

		it('removes its entry from the scope when the media input has no memories left', async () => {
			const { input, mediaScope } = await setup();

			mediaScope.setMemory(mediaPickerMemory);
			await new Promise((resolve) => setTimeout(resolve, 0));
			expect(input.interactionMemories).to.have.lengthOf(1);

			mediaScope.deleteMemory(mediaPickerMemory.unique);
			await new Promise((resolve) => setTimeout(resolve, 0));

			expect(input.interactionMemories).to.deep.equal([]);
		});
	});

	describe('changing the target of a saved link', () => {
		let host: UmbTestLinkPickerModalHostElement;
		let modal: UmbLinkPickerModalElement;
		let openedModals: Array<unknown>;
		let pickerResult: () => Promise<unknown>;

		// The media input resolves its item repository through the extension registry.
		const mediaItemRepository = mediaItemManifests.find((manifest) => manifest.type === 'repository')!;

		before(async () => {
			await useMockSet('kitchenSink');
			umbExtensionsRegistry.register(mediaItemRepository);
		});

		after(() => umbExtensionsRegistry.unregister(mediaItemRepository.alias));

		beforeEach(async () => {
			host = await fixture(html`<umb-test-link-picker-modal-host></umb-test-link-picker-modal-host>`);

			openedModals = [];
			pickerResult = () => Promise.resolve({ unique: CHECKBOX_LIST_ID, entityType: UMB_DOCUMENT_ENTITY_TYPE });
			const modalManager = {
				getHostElement: () => host,
				open: (_host: unknown, token: unknown) => {
					openedModals.push(token);
					return { onSubmit: pickerResult };
				},
			};
			host.provideContext(UMB_MODAL_MANAGER_CONTEXT, modalManager as never);
		});

		const openDialog = async (link: UmbLinkPickerLink) => {
			const value = new UmbObjectState<UmbLinkPickerModalValue>({ link });
			modal = document.createElement('umb-link-picker-modal');
			modal.modalContext = {
				value: value.asObservable(),
				size: new UmbStringState('small').asObservable(),
				updateValue: (partial: Partial<UmbLinkPickerModalValue>) => value.update(partial),
			} as unknown as UmbModalContext<UmbLinkPickerModalData, UmbLinkPickerModalValue>;
			host.appendChild(modal);
			await modal.updateComplete;
		};

		const openDocumentLink = async (name: string) => {
			await openDialog({ type: 'document', unique: HOME_ID, name, queryString: '#anchor', target: '_blank' });
			await waitUntil(
				() => modal.shadowRoot?.querySelector<UmbEntityItemRefElement>('umb-entity-item-ref')?.item,
				'picked document was not loaded',
			);
		};

		const replaceButton = (root: ShadowRoot | null | undefined = modal.shadowRoot) =>
			root?.querySelector<HTMLElement>(`uui-action-bar uui-button[label="${modal.localize.term('actions_replace')}"]`);

		it('replaces the document in place, keeping title, anchor and target', async () => {
			await openDocumentLink('My title');
			expect(replaceButton(), 'replace button').to.exist;

			replaceButton()!.click();
			await waitUntil(() => modal.value.link.unique === CHECKBOX_LIST_ID, 'document was not changed');

			expect(openedModals).to.deep.equal([UMB_DOCUMENT_LINK_PICKER_MODAL]);
			expect(modal.value.link).to.include({
				type: 'document',
				unique: CHECKBOX_LIST_ID,
				name: 'My title',
				queryString: '#anchor',
				target: '_blank',
			});
		});

		it('keeps the link and settles quietly when the picker is closed without choosing', async () => {
			await openDocumentLink('My title');
			pickerResult = () => Promise.reject(new Error('Closed'));
			const unhandled: Array<unknown> = [];
			const onUnhandledRejection = (event: PromiseRejectionEvent) => {
				unhandled.push(event.reason);
				event.preventDefault();
			};
			window.addEventListener('unhandledrejection', onUnhandledRejection);

			try {
				replaceButton()!.click();
				await waitUntil(() => openedModals.length === 1, 'picker was not opened');
				await aTimeout(50);
			} finally {
				window.removeEventListener('unhandledrejection', onUnhandledRejection);
			}

			expect(unhandled).to.be.empty;
			expect(modal.value.link).to.include({ type: 'document', unique: HOME_ID, name: 'My title' });
		});

		it('updates a title that is the document name when the document is replaced', async () => {
			await openDocumentLink('Home');

			replaceButton()!.click();
			await waitUntil(() => modal.value.link.unique === CHECKBOX_LIST_ID, 'document was not changed');

			expect(modal.value.link).to.include({ unique: CHECKBOX_LIST_ID, name: 'Checkbox List' });
		});

		it('updates a title that is the media name when the media item is replaced', async () => {
			pickerResult = () => Promise.resolve({ selection: [OTHER_IMAGE_ID] });
			await openDialog({ type: 'media', unique: IMAGE_ID, name: 'Placeholder 73640' });
			const mediaInput = modal.shadowRoot!.querySelector<UmbInputMediaElement>('umb-input-media')!;
			await waitUntil(() => replaceButton(mediaInput.shadowRoot), 'picked media was not loaded');

			replaceButton(mediaInput.shadowRoot)!.click();
			await waitUntil(() => modal.value.link.unique === OTHER_IMAGE_ID, 'media was not changed');

			expect(modal.value.link).to.include({ unique: OTHER_IMAGE_ID, name: 'Placeholder 5085407' });
		});

		describe('manual URL', () => {
			const enterUrl = async (url: string) => {
				const urlInput = modal.shadowRoot!.querySelector<HTMLInputElement>('[data-mark="input:url"]')!;
				urlInput.value = url;
				urlInput.dispatchEvent(new Event('input'));
				await modal.updateComplete;
			};

			it('updates a title that is the host name when the URL changes', async () => {
				await openDialog({ type: 'external', url: 'https://umbraco.com/products', name: 'umbraco.com' });

				await enterUrl('https://example.com/about');

				expect(modal.value.link).to.include({ url: 'https://example.com/about', name: 'example.com' });
			});

			it('keeps a title of its own when the URL changes', async () => {
				await openDialog({ type: 'external', url: 'https://umbraco.com/products', name: 'Our products' });

				await enterUrl('https://example.com/about');

				expect(modal.value.link).to.include({ url: 'https://example.com/about', name: 'Our products' });
			});
		});
	});
});
