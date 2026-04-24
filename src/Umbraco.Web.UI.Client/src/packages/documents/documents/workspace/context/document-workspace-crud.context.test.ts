import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UmbDocumentDetailStore } from '../../repository/detail/document-detail.store.js';
import { manifests as documentDetailRepositoryManifests } from '../../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../../document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../../../data-type/repository/detail/data-type-detail.store.js';
import { UmbDataTypeItemStore } from '../../../../data-type/repository/item/data-type-item.store.js';
import { manifests as userPermissionConditionManifests } from '../../user-permissions/document/conditions/manifests.js';
import { manifests as dataTypeItemManifests } from '../../../../data-type/repository/item/manifests.js';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';

const PARENT_ENTITY = { entityType: 'document', unique: null } as const;

const TEST_MANIFESTS = [
	...documentDetailRepositoryManifests,
	...userPermissionConditionManifests,
	...dataTypeItemManifests,
];

@customElement('umb-test-document-workspace-crud-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentDetailStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDataTypeDetailStore(this);
		new UmbDataTypeItemStore(this);
		new UmbActionEventContext(this);
	}
}

describe('UmbDocumentWorkspaceContext (CRUD)', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbDocumentWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('variantDocuments');
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		context = new UmbDocumentWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('createScaffold', () => {
		beforeEach(async () => {
			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
		});

		it('sets isNew to true', () => {
			expect(context.getIsNew()).to.be.true;
		});

		it('assigns a unique identifier', () => {
			expect(context.getUnique()).to.be.a('string').and.not.be.empty;
		});

		it('populates the document type unique', () => {
			expect(context.getData()?.documentType.unique).to.equal(INVARIANT_DOCUMENT_TYPE_ID);
		});

		it('initializes with no variants', () => {
			expect(context.getData()?.variants).to.be.an('array').with.lengthOf(0);
		});

		it('initializes with no values', () => {
			expect(context.getData()?.values).to.be.an('array').with.lengthOf(0);
		});
	});

	describe('create (save new document)', () => {
		let uniqueBeforeSave: string | undefined;

		beforeEach(async () => {
			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
			context.setName('New Test Document');
			uniqueBeforeSave = context.getUnique();
			await context.requestSave();
		});

		it('sets isNew to false after saving', () => {
			expect(context.getIsNew()).to.be.false;
		});

		it('retains the same unique after saving', () => {
			expect(context.getUnique()).to.equal(uniqueBeforeSave);
		});

		it('persists the document name', () => {
			const variant = context.getData()?.variants[0];
			expect(variant?.name).to.equal('New Test Document');
		});

		it('can be reloaded after creation', async () => {
			const unique = context.getUnique()!;
			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(unique);
			expect(newContext.getData()?.documentType.unique).to.equal(INVARIANT_DOCUMENT_TYPE_ID);
		});
	});

	describe('update (save existing document)', () => {
		beforeEach(async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
		});

		it('persists updated property values after save', async () => {
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();
			expect(context.getPropertyValue('text')).to.equal('Updated by test');
		});

		it('does not alter the variant name when saving property changes', async () => {
			const nameBefore = context.getData()?.variants[0]?.name;
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();
			expect(context.getData()?.variants[0]?.name).to.equal(nameBefore);
		});

		it('reflects the saved value on a fresh load', async () => {
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(INVARIANT_DOCUMENT_ID);
			expect(newContext.getPropertyValue('text')).to.equal('Updated by test');
		});
	});

	describe('delete', () => {
		it('removes the document so a subsequent load returns an error', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			await context.delete(INVARIANT_DOCUMENT_ID);

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			const { error } = await newContext.load(INVARIANT_DOCUMENT_ID);
			expect(error).to.exist;
		});
	});
});
