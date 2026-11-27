import { manifests as documentBlueprintDetailRepositoryManifests } from '../repository/detail/manifests.js';
import { UmbDocumentBlueprintDetailStore } from '../repository/detail/document-blueprint-detail.store.js';
import {
	TEST_MANIFESTS,
	UmbTestDocumentWorkspaceHostElement,
} from '../../documents/workspace/context/document-workspace-context.test-utils.js';
import { UmbDocumentBlueprintWorkspaceContext } from './document-blueprint-workspace.context.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

const SEGMENT_VARIANT_DOCUMENT_BLUEPRINT_ID = 'variant-documents-segment-variant-document-blueprint-id';

const MANIFESTS = [...TEST_MANIFESTS, ...documentBlueprintDetailRepositoryManifests];

describe('UmbDocumentBlueprintWorkspaceContext', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentBlueprintWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		new UmbDocumentBlueprintDetailStore(hostElement);
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentBlueprintWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('variant options', () => {
		it('offers every available segment for a blueprint that varies by segment', async () => {
			await context.load(SEGMENT_VARIANT_DOCUMENT_BLUEPRINT_ID);

			const variantOptions = await context.getVariantOptions();

			expect(variantOptions.map((option) => option.segment)).to.deep.equal([null, 's1', 's2']);
		});

		it('links a segment option to the blueprint variant that exists for it', async () => {
			await context.load(SEGMENT_VARIANT_DOCUMENT_BLUEPRINT_ID);

			const variantOptions = await context.getVariantOptions();

			expect(variantOptions.find((option) => option.segment === 's1')?.variant?.segment).to.equal('s1');
			expect(variantOptions.find((option) => option.segment === 's2')?.variant).to.be.undefined;
		});
	});
});
