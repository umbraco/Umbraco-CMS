import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
const PARENT_ENTITY = { entityType: 'document', unique: null } as const;

describe('UmbDocumentWorkspaceContext (loading)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;

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
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('isLoaded', () => {
		it('resolves once the data has been applied, when called right after load has started', async () => {
			context.load(INVARIANT_DOCUMENT_ID);
			expect(context.getData()).to.be.undefined;

			await context.isLoaded();

			expect(context.getData()?.unique).to.equal(INVARIANT_DOCUMENT_ID);
		});

		it('resolves once the data has been applied, when called right after createScaffold has started', async () => {
			context.createScaffold({
				parent: PARENT_ENTITY,
				preset: { documentType: { unique: INVARIANT_DOCUMENT_TYPE_ID } } as never,
			});

			await context.isLoaded();

			expect(context.getData()).to.not.be.undefined;
		});

		it('resolves when the load fails', async () => {
			const loading = context.load('does-not-exist').catch(() => undefined);

			await context.isLoaded();
			await loading;

			expect(context.getData()).to.be.undefined;
		});

		it('resolves when processing the incoming data throws', async () => {
			context.incomingDataHook.add(() => {
				throw new Error('hook failed');
			});

			const loading = context.load(INVARIANT_DOCUMENT_ID).catch(() => undefined);

			await context.isLoaded();
			await loading;

			expect(context.getData()).to.be.undefined;
		});
	});

	describe('hooks', () => {
		it('awaits the loading hook before the incoming data hook runs', async () => {
			const log: Array<string> = [];
			context.loadingHook.add(async () => {
				await new Promise((resolve) => setTimeout(resolve, 20));
				log.push('loading');
			});
			context.incomingDataHook.add((data) => {
				log.push('incoming');
				return data;
			});

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.deep.equal(['loading', 'incoming']);
		});

		it('applies the data returned by the incoming data hook', async () => {
			context.incomingDataHook.add((data) => ({ ...data, isTrashed: true }));

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(context.getData()?.isTrashed).to.be.true;
		});

		it('runs the incoming data hook on reload', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			let calls = 0;
			context.incomingDataHook.add((data) => {
				calls++;
				return data;
			});

			await context.reload();

			expect(calls).to.equal(1);
		});
	});
});
