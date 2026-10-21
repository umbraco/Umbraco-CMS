import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbElementPublishingWorkspaceContext } from './element-publishing.workspace-context.js';
import { UmbElementWorkspaceContext } from '../../workspace/element-workspace.context.js';
import {
	TEST_MANIFESTS,
	UmbTestElementWorkspaceHostElement,
} from '../../workspace/element-workspace-context.test-utils.js';
import { UMB_CONTENT_PUBLISH_MODAL } from '@umbraco-cms/backoffice/content';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbElementPublishingServerDataSource } from '../repository/element-publishing.server.data-source.js';
import { UmbElementReferenceRepository } from '../../reference/repository/element-reference.repository.js';
import type {
	ManifestEntityPublishAwareness,
	UmbEntityPublishAwarenessApi,
} from '@umbraco-cms/backoffice/content';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueEntityReferenceResolver,
} from '@umbraco-cms/backoffice/property';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

// Not simple-element-id: the mock referenced-by handler hardcodes that one to have references
// (used elsewhere to test the "has references" case), which would defeat the "no modal" test below.
const ELEMENT_ID = 'element-in-folder-id';

describe('UmbElementPublishingWorkspaceContext', () => {
	let hostElement: UmbTestElementWorkspaceHostElement;
	let context: UmbElementWorkspaceContext;
	let publishingContext: UmbElementPublishingWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('default');
		hostElement = new UmbTestElementWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbElementWorkspaceContext(hostElement);
		// The workspace element hosts additional workspace contexts on the workspace API itself, so that
		// contexts sharing a context alias resolve against each other correctly. (see UmbWorkspaceElement)
		publishingContext = new UmbElementPublishingWorkspaceContext(context);
		await context.load(ELEMENT_ID);
		await aTimeout(0);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('save and publish — reference-awareness gating for single-variant elements', () => {
		/**
		 * Tracks whether UMB_CONTENT_PUBLISH_MODAL was opened while `run` executes, and immediately rejects
		 * every modal so the flow bails out early (mirrors the cancel path) without needing full rendering.
		 */
		async function runSaveAndPublishTrackingModal(run: () => Promise<unknown>) {
			let opened = false;
			const originalOpen = UmbModalManagerContext.prototype.open;
			UmbModalManagerContext.prototype.open = function (
				this: UmbModalManagerContext,
				...args: Parameters<typeof originalOpen>
			) {
				if (String(args[1]) === UMB_CONTENT_PUBLISH_MODAL.toString()) opened = true;
				const modalContext = originalOpen.apply(this, args as never);
				modalContext.reject();
				return modalContext;
			} as typeof originalOpen;

			try {
				await run();
			} finally {
				UmbModalManagerContext.prototype.open = originalOpen;
			}

			return opened;
		}

		let restores: Array<() => void> = [];

		afterEach(() => {
			restores.forEach((restore) => restore());
			restores = [];
		});

		it('publishes immediately, with no modal, when nothing references this element and no referenced element has pending changes', async () => {
			let published = false;
			const originalUpdateAndPublish = UmbElementPublishingServerDataSource.prototype.updateAndPublish;
			restores.push(() => {
				UmbElementPublishingServerDataSource.prototype.updateAndPublish = originalUpdateAndPublish;
			});
			UmbElementPublishingServerDataSource.prototype.updateAndPublish = async function (...args) {
				published = true;
				return originalUpdateAndPublish.apply(this, args as never);
			};

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.false;
			expect(published, 'publish went through').to.be.true;
		});

		it('opens the modal when something references this element', async () => {
			const original = UmbElementReferenceRepository.prototype.requestReferencedBy;
			restores.push(() => {
				UmbElementReferenceRepository.prototype.requestReferencedBy = original;
			});
			UmbElementReferenceRepository.prototype.requestReferencedBy = async () => ({
				data: { items: [], total: 1 },
			});

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.true;
		});

		it('opens the modal when this element references another element that is not fully published', async () => {
			// ELEMENT_ID's own property value is a plain `Umbraco.TextBox` — fake a `propertyValueEntityReference`
			// resolver for it so the draft "references" a fake element, plus the `entityPublishAwareness` +
			// item-repository plumbing that turns that reference into a real "needs attention" item.
			const ITEM_REPOSITORY_ALIAS = 'Umb.Test.PublishingWorkspaceContext.ItemRepository';

			class TestEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
				async resolveEntityReferences(): Promise<Array<UmbEntityModel>> {
					return [{ entityType: 'element', unique: 'other-element-id' }];
				}
				destroy(): void {}
			}
			class TestItemRepository implements UmbItemRepository<UmbEntityModel> {
				async requestItems(uniques: Array<string>) {
					return { data: uniques.map((unique) => ({ entityType: 'element', unique })) };
				}
				destroy(): void {}
			}
			class TestPublishAwarenessApi implements UmbEntityPublishAwarenessApi<UmbEntityModel> {
				needsAttention(): boolean {
					return true;
				}
				destroy(): void {}
			}

			const entityReferenceManifest: ManifestPropertyValueEntityReference = {
				type: 'propertyValueEntityReference',
				name: 'Test Entity Reference Resolver',
				alias: 'Umb.Test.PublishingWorkspaceContext.EntityReferenceResolver',
				api: TestEntityReferenceResolver,
				forEditorAlias: 'Umbraco.TextBox',
			};
			const itemRepositoryManifest: ManifestApi<TestItemRepository> = {
				type: 'repository',
				name: 'Test Item Repository',
				alias: ITEM_REPOSITORY_ALIAS,
				api: TestItemRepository,
			};
			const publishAwarenessManifest: ManifestEntityPublishAwareness = {
				type: 'entityPublishAwareness',
				name: 'Test Entity Publish Awareness',
				alias: 'Umb.Test.PublishingWorkspaceContext.PublishAwareness',
				api: TestPublishAwarenessApi,
				forEntityTypes: ['element'],
				meta: { itemRepositoryAlias: ITEM_REPOSITORY_ALIAS },
			};

			umbExtensionsRegistry.register(entityReferenceManifest);
			umbExtensionsRegistry.register(itemRepositoryManifest);
			umbExtensionsRegistry.register(publishAwarenessManifest);
			restores.push(() => {
				umbExtensionsRegistry.unregister(entityReferenceManifest.alias);
				umbExtensionsRegistry.unregister(ITEM_REPOSITORY_ALIAS);
				umbExtensionsRegistry.unregister(publishAwarenessManifest.alias);
			});

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.true;
		});

		it('opens the modal rather than risk publishing silently when a reference count lookup fails', async () => {
			const original = UmbElementReferenceRepository.prototype.requestReferencedBy;
			restores.push(() => {
				UmbElementReferenceRepository.prototype.requestReferencedBy = original;
			});
			UmbElementReferenceRepository.prototype.requestReferencedBy = async () => {
				throw new Error('Simulated network error');
			};

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.true;
		});
	});
});
