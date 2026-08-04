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

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });

/**
 * Records the aliases of every modal opened while `run` executes. The test host auto-submits modals,
 * so what a flow asks for is observable, but the user's answer is not. Failures are swallowed: the
 * unpublish entity action needs contexts the test host does not provide, and it runs after the point
 * these tests are about.
 */
async function recordOpenedModals(run: () => Promise<unknown>): Promise<Array<string>> {
	const aliases: Array<string> = [];
	const originalOpen = UmbModalManagerContext.prototype.open;
	UmbModalManagerContext.prototype.open = function (
		this: UmbModalManagerContext,
		...args: Parameters<typeof originalOpen>
	) {
		aliases.push(String(args[1]));
		return originalOpen.apply(this, args as never);
	} as typeof originalOpen;

	try {
		await run().catch(() => undefined);
	} finally {
		UmbModalManagerContext.prototype.open = originalOpen;
	}

	return aliases;
}

describe('UmbDocumentPublishingWorkspaceContext', () => {
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
	});

	describe('unpublish', () => {
		// Unpublishing reloads the workspace, which throws away unsaved edits, so the user gets a say.
		it('asks to discard unsaved changes', async () => {
			await context.setName('Renamed root', EN_US);

			const opened = await recordOpenedModals(() => publishingContext.unpublish());

			expect(opened).to.include(UMB_DISCARD_CHANGES_MODAL.toString());
		});

		it('does not ask when there is nothing to discard', async () => {
			const opened = await recordOpenedModals(() => publishingContext.unpublish());

			expect(opened).to.not.include(UMB_DISCARD_CHANGES_MODAL.toString());
		});
	});
});
