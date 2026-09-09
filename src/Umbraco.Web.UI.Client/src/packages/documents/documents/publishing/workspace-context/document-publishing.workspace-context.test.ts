import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentPublishingWorkspaceContext } from './document-publishing.workspace-context.js';
import { UmbDocumentWorkspaceContext } from '../../workspace/context/document-workspace.context.js';
import {
	TEST_MANIFESTS,
	UmbTestDocumentWorkspaceHostElement,
} from '../../workspace/context/document-workspace-context.test-utils.js';
import { UMB_DISCARD_CHANGES_MODAL, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbDocumentPublishingServerDataSource } from '../repository/document-publishing.server.data-source.js';
import { UMB_CONTENT_PUBLISH_MODAL, UmbContentUnpublishEntityAction } from '@umbraco-cms/backoffice/content';
import type {
	ManifestEntityPublishAwareness,
	UmbEntityPublishAwarenessApi,
} from '@umbraco-cms/backoffice/content';
import { UmbDocumentReferenceRepository } from '../../reference/repository/document-reference.repository.js';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueEntityReferenceResolver,
} from '@umbraco-cms/backoffice/property';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });
const DA = UmbVariantId.Create({ culture: 'da', segment: null });

/**
 * Forces the value every modal is submitted with while `run` executes. The test host's modal manager
 * submits with whatever value the context holds when `open` returns, so setting it here decides the
 * answer — which the auto-submitting mock alone cannot express.
 */
async function answerModalsWith(value: unknown, run: () => Promise<unknown>): Promise<void> {
	const originalOpen = UmbModalManagerContext.prototype.open;
	UmbModalManagerContext.prototype.open = function (
		this: UmbModalManagerContext,
		...args: Parameters<typeof originalOpen>
	) {
		const modalContext = originalOpen.apply(this, args as never);
		modalContext.setValue(value as never);
		return modalContext;
	} as typeof originalOpen;

	try {
		await run();
	} finally {
		UmbModalManagerContext.prototype.open = originalOpen;
	}
}

/**
 * Counts calls to the publish-with-descendants endpoint while `run` executes.
 */
async function countPublishWithDescendantsCalls(run: () => Promise<unknown>): Promise<number> {
	let calls = 0;
	const originalPublish = UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants;
	UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants = function (...args) {
		calls++;
		return originalPublish.apply(this, args as never);
	};

	try {
		await run();
	} finally {
		UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants = originalPublish;
	}

	return calls;
}

/**
 * Makes the publish-with-descendants endpoint fail while `run` executes, leaving the preceding save
 * to succeed — the split Andy Butland reproduced by returning BadRequest from the controller.
 */
async function withFailingPublish(run: () => Promise<unknown>): Promise<void> {
	const originalPublish = UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants;
	UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants = async () => ({
		error: new Error('Simulated publish failure'),
	});

	try {
		await run();
	} finally {
		UmbDocumentPublishingServerDataSource.prototype.publishWithDescendants = originalPublish;
	}
}

describe('UmbDocumentPublishingWorkspaceContext', function () {
	// The publish-with-descendants mock polls once with a one second delay before completing.
	this.timeout(10000);

	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;
	let publishingContext: UmbDocumentPublishingWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentWorkspaceContext(hostElement);
		// The workspace element hosts additional workspace contexts on the workspace API itself, so that
		// contexts sharing a context alias resolve against each other correctly. (see UmbWorkspaceElement)
		publishingContext = new UmbDocumentPublishingWorkspaceContext(context);
		await context.load(VARIANT_DOCUMENT_ID);
		await aTimeout(0);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('publish with descendants', () => {
		// The action used to publish descendants without saving the document it was invoked from, so any
		// edit made in the workspace was silently discarded by the reload that follows. (#14925)
		it('saves the document it is invoked from', async () => {
			await context.setName('Renamed root', EN_US);
			await context.setPropertyValue('variantText', 'Edited English', EN_US);

			await publishingContext.publishWithDescendants();

			const freshContext = new UmbDocumentWorkspaceContext(hostElement);
			await freshContext.load(VARIANT_DOCUMENT_ID);
			expect(freshContext.getName(EN_US), 'name saved on server').to.equal('Renamed root');
			expect(freshContext.getPropertyValue('variantText', EN_US), 'property saved on server').to.equal(
				'Edited English',
			);
		});

		it('keeps the edit in the workspace after the reload that follows', async () => {
			await context.setName('Renamed root', EN_US);

			await publishingContext.publishWithDescendants();

			expect(context.getName(EN_US)).to.equal('Renamed root');
		});

		// Saving merges the server response into the published variants only, so an edit in a variant that
		// was not selected must survive rather than be replaced by a wholesale reload.
		it('keeps edits in variants that were not published', async () => {
			await context.setPropertyValue('variantText', 'Edited English', EN_US);
			await context.setPropertyValue('variantText', 'Redigeret dansk', DA);

			await answerModalsWith({ selection: [EN_US.toString()], includeUnpublishedDescendants: false }, () =>
				publishingContext.publishWithDescendants(),
			);

			expect(context.getPropertyValue('variantText', DA), 'da edit still in the editor').to.equal('Redigeret dansk');
			expect(
				context.getChangedVariants().some((v) => v.culture === 'da'),
				'da is still reported as changed',
			).to.be.true;
		});

		// The save and the publish are separate calls, so a failing publish must still leave the
		// document saved and the workspace clean rather than dirty against a stale snapshot. (#14925)
		it('leaves the document saved when only the publish fails', async () => {
			await context.setName('Renamed root', EN_US);

			let rejected = false;
			await withFailingPublish(() =>
				publishingContext.publishWithDescendants().then(
					() => undefined,
					() => {
						rejected = true;
					},
				),
			);

			expect(rejected, 'the action reports failure').to.be.true;
			expect(context.getHasUnpersistedChanges(), 'workspace is not left dirty').to.be.false;

			const freshContext = new UmbDocumentWorkspaceContext(hostElement);
			await freshContext.load(VARIANT_DOCUMENT_ID);
			expect(freshContext.getName(EN_US), 'the save survived the failed publish').to.equal('Renamed root');
		});

		// Saving first means the save's mandatory validation now gates the whole operation.
		it('publishes nothing when the document fails mandatory validation', async () => {
			await context.setName('', EN_US);

			const publishCalls = await countPublishWithDescendantsCalls(() =>
				publishingContext.publishWithDescendants().then(
					() => undefined,
					() => undefined,
				),
			);

			expect(publishCalls, 'descendants were not published').to.equal(0);
		});
	});

	describe('unpublish', () => {
		/**
		 * Stubs out the unpublish entity action — it needs contexts the test host does not provide — so
		 * whether the flow got past the guard is observable, and optionally answers every modal with cancel.
		 */
		async function runUnpublish(options: { cancelModals?: boolean } = {}) {
			const modals: Array<string> = [];
			let reachedUnpublish = false;

			const originalOpen = UmbModalManagerContext.prototype.open;
			UmbModalManagerContext.prototype.open = function (
				this: UmbModalManagerContext,
				...args: Parameters<typeof originalOpen>
			) {
				modals.push(String(args[1]));
				const modalContext = originalOpen.apply(this, args as never);
				if (options.cancelModals) {
					modalContext.reject();
				}
				return modalContext;
			} as typeof originalOpen;

			const originalExecute = UmbContentUnpublishEntityAction.prototype.executeWithResult;
			UmbContentUnpublishEntityAction.prototype.executeWithResult = async function () {
				reachedUnpublish = true;
				return false;
			};

			try {
				await publishingContext.unpublish();
			} finally {
				UmbModalManagerContext.prototype.open = originalOpen;
				UmbContentUnpublishEntityAction.prototype.executeWithResult = originalExecute;
			}

			return { modals, reachedUnpublish };
		}

		// Unpublishing reloads the workspace, which throws away unsaved edits, so the user gets a say.
		it('asks to discard unsaved changes, and unpublishes once discarded', async () => {
			await context.setName('Renamed root', EN_US);

			const { modals, reachedUnpublish } = await runUnpublish();

			expect(modals, 'asked to discard').to.include(UMB_DISCARD_CHANGES_MODAL.toString());
			expect(reachedUnpublish, 'went on to unpublish').to.be.true;
		});

		it('does not unpublish when the discard is cancelled', async () => {
			await context.setName('Renamed root', EN_US);

			const { modals, reachedUnpublish } = await runUnpublish({ cancelModals: true });

			expect(modals, 'asked to discard').to.include(UMB_DISCARD_CHANGES_MODAL.toString());
			expect(reachedUnpublish, 'stopped before unpublishing').to.be.false;
			expect(context.getName(EN_US), 'the edit is untouched').to.equal('Renamed root');
		});

		it('does not ask when there is nothing to discard', async () => {
			const { modals, reachedUnpublish } = await runUnpublish();

			expect(modals, 'no discard prompt').to.not.include(UMB_DISCARD_CHANGES_MODAL.toString());
			expect(reachedUnpublish, 'went straight to unpublishing').to.be.true;
		});
	});

	describe('save and publish — reference-awareness gating for single-variant documents', () => {
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

		beforeEach(async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			await aTimeout(0);
		});

		afterEach(() => {
			restores.forEach((restore) => restore());
			restores = [];
		});

		it('publishes immediately, with no modal, when nothing references this document and no referenced element has pending changes', async () => {
			let published = false;
			const originalUpdateAndPublish = UmbDocumentPublishingServerDataSource.prototype.updateAndPublish;
			restores.push(() => {
				UmbDocumentPublishingServerDataSource.prototype.updateAndPublish = originalUpdateAndPublish;
			});
			UmbDocumentPublishingServerDataSource.prototype.updateAndPublish = async function (...args) {
				published = true;
				return originalUpdateAndPublish.apply(this, args as never);
			};

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.false;
			expect(published, 'publish went through').to.be.true;
		});

		it('opens the modal when something references this document', async () => {
			const original = UmbDocumentReferenceRepository.prototype.requestReferencedBy;
			restores.push(() => {
				UmbDocumentReferenceRepository.prototype.requestReferencedBy = original;
			});
			UmbDocumentReferenceRepository.prototype.requestReferencedBy = async () => ({
				data: { items: [], total: 1 },
			});

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.true;
		});

		it('opens the modal when this document references an element that is not fully published', async () => {
			// INVARIANT_DOCUMENT_ID's own property value is a plain `Umbraco.TextBox` — fake a
			// `propertyValueEntityReference` resolver for it so the draft "references" a fake element, plus the
			// `entityPublishAwareness` + item-repository plumbing that turns that reference into a real
			// "needs attention" item.
			const ITEM_REPOSITORY_ALIAS = 'Umb.Test.DocumentPublishingWorkspaceContext.ItemRepository';

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
				alias: 'Umb.Test.DocumentPublishingWorkspaceContext.EntityReferenceResolver',
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
				alias: 'Umb.Test.DocumentPublishingWorkspaceContext.PublishAwareness',
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
			const original = UmbDocumentReferenceRepository.prototype.requestReferencedBy;
			restores.push(() => {
				UmbDocumentReferenceRepository.prototype.requestReferencedBy = original;
			});
			UmbDocumentReferenceRepository.prototype.requestReferencedBy = async () => {
				throw new Error('Simulated network error');
			};

			const opened = await runSaveAndPublishTrackingModal(() => publishingContext.saveAndPublish());
			expect(opened, 'modal opened').to.be.true;
		});
	});
});
