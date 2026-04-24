import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UmbDocumentDetailStore } from '../repository/detail/document-detail.store.js';
import { manifests as documentDetailRepositoryManifests } from '../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../../data-type/repository/detail/data-type-detail.store.js';
import { manifests as userPermissionConditionManifests } from '../user-permissions/document/conditions/manifests.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

const TEST_MANIFESTS = [...documentDetailRepositoryManifests, ...userPermissionConditionManifests];

@customElement('umb-test-document-workspace-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentDetailStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDataTypeDetailStore(this);
	}
}

describe('UmbDocumentWorkspaceContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbDocumentWorkspaceContext;

	before(async () => {
		await useMockSet('variantDocuments');
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		context = new UmbDocumentWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('getPropertyValue', () => {
		describe('invariant document', () => {
			beforeEach(async () => {
				await context.load(INVARIANT_DOCUMENT_ID);
			});

			it('returns the invariant property value', () => {
				expect(context.getPropertyValue('text')).to.equal('This is the invariant text value.');
			});

			it('returns undefined for an unknown alias', () => {
				expect(context.getPropertyValue('nonExistent')).to.be.undefined;
			});
		});

		describe('variant document', () => {
			beforeEach(async () => {
				await context.load(VARIANT_DOCUMENT_ID);
			});

			it('returns the invariant property value without a variantId', () => {
				expect(context.getPropertyValue('text')).to.equal('This invariant text is shared across all cultures.');
			});

			it('returns the en-US variant property value', () => {
				const variantId = UmbVariantId.Create({ culture: 'en-US', segment: null });
				expect(context.getPropertyValue('variantText', variantId)).to.equal('This is the English variant text.');
			});

			it('returns the da variant property value', () => {
				const variantId = UmbVariantId.Create({ culture: 'da', segment: null });
				expect(context.getPropertyValue('variantText', variantId)).to.equal('Dette er den danske varianttekst.');
			});

			it('returns undefined for an unknown alias', () => {
				expect(context.getPropertyValue('nonExistent')).to.be.undefined;
			});
		});
	});
});
