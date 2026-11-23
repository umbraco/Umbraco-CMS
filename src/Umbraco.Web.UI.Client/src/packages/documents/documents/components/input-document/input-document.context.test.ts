import { UmbDocumentPickerInputContext } from './input-document.context.js';
import type { UmbDocumentPickerInputSelectedItemAncestorsModel } from './input-document.context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentVariantState } from '../../variant-state.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import { UmbPropertyTypeBasedPropertyContext } from '@umbraco-cms/backoffice/content';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import { UmbDocumentTreeRepository } from '../../tree/document-tree.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

// The picker holds items as UmbDocumentItemModel widened with a name, see the HACK on
// UmbDocumentPickerInputContext.
type UmbTestPickedDocumentItemModel = UmbDocumentItemModel & { name: string };

@customElement('test-document-picker-input-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Exposes the protected _requestItemName method and bypasses the item manager so
// tests do not need the repository extension to be registered.
class UmbTestDocumentPickerInputContext extends UmbDocumentPickerInputContext {
	#testItems = new Map<string, UmbTestPickedDocumentItemModel>();

	setTestItem(item: UmbTestPickedDocumentItemModel) {
		this.#testItems.set(item.unique, item);
	}

	override getSelectedItemByUnique(unique: string) {
		return this.#testItems.get(unique);
	}

	async requestItemName(unique: string): Promise<string> {
		return this._requestItemName(unique);
	}
}

function makeItem(unique: string, name: string): UmbTestPickedDocumentItemModel {
	return {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		unique,
		name,
		documentType: {
			unique: 'document-type-unique',
			icon: 'icon-document',
		},
		hasChildren: false,
		isProtected: false,
		isTrashed: false,
		parent: null,
		flags: [],
		variants: [
			{
				name,
				culture: 'en-US',
				state: UmbDocumentVariantState.PUBLISHED,
				flags: [],
			},
		],
	};
}

describe('UmbDocumentPickerInputContext._requestItemName', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestDocumentPickerInputContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		context = new UmbTestDocumentPickerInputContext(hostElement);
	});

	afterEach(() => {
		context.destroy();
		document.body.innerHTML = '';
	});

	it('should return the variant name of a selected item', async () => {
		context.setTestItem(makeItem('document-1', 'Document One'));

		const name = await context.requestItemName('document-1');
		expect(name).to.equal('Document One');
	});

	it('should resolve to the not found label when the unique has no item', async () => {
		// A selection can reference a deleted document. The name must still resolve, otherwise
		// everything awaiting it - such as the remove confirmation dialog - never happens.
		const name = await context.requestItemName('does-not-exist');
		expect(name).to.equal('#general_notFound');
	});
});

describe('UmbDocumentPickerInputContext selectedItemsAncestors', () => {
	let hostElement: UmbTestControllerHostElement;
	let actionEventContext: UmbActionEventContext;
	let context: UmbDocumentPickerInputContext;
	let ancestors: Array<UmbDocumentPickerInputSelectedItemAncestorsModel>;
	let ancestorRequests: Array<Array<string>>;
	let ancestorRequestDataTypeIds: Array<string | undefined>;
	let parentOf: Record<string, string>;
	let treeItemNames: Record<string, string>;
	let missingItems: Set<string>;
	let itemsWithoutAncestors: Set<string>;
	let restoreTreeRepository: () => void;
	let ancestorRequestEntityTypes: Array<string>;

	const originalAncestorsBatch = DocumentService.getTreeDocumentAncestorsBatch;

	class UmbTestDocumentItemRepository extends UmbControllerBase {
		async requestItems(uniques: Array<string>) {
			return {
				data: uniques.filter((unique) => !missingItems.has(unique)).map((unique) => makeItem(unique, unique)),
			};
		}
	}

	const makeTreeItemResponse = (id: string) => ({
		id,
		ancestors: parentOf[id] ? [{ id: parentOf[id] }] : [],
		parent: parentOf[id] ? { id: parentOf[id] } : null,
		documentType: { id: 'document-type-unique', icon: 'icon-document', collection: null },
		noAccess: false,
		isTrashed: false,
		hasChildren: false,
		isProtected: false,
		flags: [],
		createDate: '',
		variants: [{ name: treeItemNames[id] ?? id, culture: 'en-US', state: 'Published', flags: [] }],
	});

	const ancestorNamesOf = (unique: string) =>
		ancestors.find((entry) => entry.treeItem.unique === unique)?.ancestors?.map((item) => item.variants[0].name);

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		actionEventContext = new UmbActionEventContext(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		if (umbExtensionsRegistry.isRegistered(UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS)) {
			umbExtensionsRegistry.unregister(UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS);
		}
		umbExtensionsRegistry.register({
			type: 'repository',
			alias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			name: 'Test Document Item Repository',
			api: UmbTestDocumentItemRepository,
		});

		ancestorRequests = [];
		ancestorRequestDataTypeIds = [];
		parentOf = { 'document-1': 'folder-a', 'document-2': 'folder-b', 'document-3': 'folder-a' };
		treeItemNames = { 'folder-a': 'Folder A', 'folder-b': 'Folder B' };
		missingItems = new Set();
		itemsWithoutAncestors = new Set();
		ancestorRequestEntityTypes = [];
		(DocumentService as any).getTreeDocumentAncestorsBatch = (options: {
			query: { id: Array<string>; dataTypeId?: string };
		}) => {
			ancestorRequests.push([...options.query.id]);
			ancestorRequestDataTypeIds.push(options.query.dataTypeId);
			const items = options.query.id
				.filter((id) => !itemsWithoutAncestors.has(id))
				.map((id) => ({
					id,
					ancestors: parentOf[id] ? [makeTreeItemResponse(parentOf[id])] : [],
				}));
			return Promise.resolve({ data: { total: items.length, items } });
		};

		const originalRequestAncestorsOfItems = UmbDocumentTreeRepository.prototype.requestAncestorsOfTreeItems;
		UmbDocumentTreeRepository.prototype.requestAncestorsOfTreeItems = function (args) {
			ancestorRequestEntityTypes.push(...args.treeItems.map((treeItem) => treeItem.entityType));
			return originalRequestAncestorsOfItems.call(this, args);
		};
		restoreTreeRepository = () =>
			(UmbDocumentTreeRepository.prototype.requestAncestorsOfTreeItems = originalRequestAncestorsOfItems);

		context = new UmbDocumentPickerInputContext(hostElement);
		ancestors = [];
		context.observe(context.selectedItemsAncestors, (value) => (ancestors = value));
	});

	afterEach(() => {
		(DocumentService as any).getTreeDocumentAncestorsBatch = originalAncestorsBatch;
		restoreTreeRepository();
		umbExtensionsRegistry.unregister(UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS);
		context.destroy();
		actionEventContext.destroy();
		document.body.innerHTML = '';
	});

	it('requests the ancestors of all selected documents in one request', async () => {
		context.setSelection(['document-1', 'document-2']);

		await waitUntil(() => ancestors.length === 2);
		expect(ancestorRequests).to.have.lengthOf(1);
		expect(ancestorRequests[0]).to.have.members(['document-1', 'document-2']);
		expect(ancestorNamesOf('document-2')).to.deep.equal(['Folder B']);
	});

	it('requests ancestors with the entity type of the selected items', async () => {
		context.setSelection(['document-1']);

		await waitUntil(() => ancestors.length === 1);
		expect(ancestorRequestEntityTypes).to.deep.equal([UMB_DOCUMENT_ENTITY_TYPE]);
	});

	it('does not request ancestors for selected documents that do not resolve to an item', async () => {
		missingItems.add('document-2');

		context.setSelection(['document-1', 'document-2']);

		await waitUntil(() => ancestors.length === 1);
		expect(ancestorRequests).to.deep.equal([['document-1']]);
	});

	it('only requests ancestors for newly selected documents', async () => {
		context.setSelection(['document-1']);
		await waitUntil(() => ancestors.length === 1);

		context.setSelection(['document-1', 'document-2']);
		await waitUntil(() => ancestors.length === 2);

		expect(ancestorRequests).to.deep.equal([['document-1'], ['document-2']]);
	});

	it('does not request the ancestors again for a selected document that has none to return', async () => {
		itemsWithoutAncestors.add('document-2');
		context.setSelection(['document-1', 'document-2']);
		await waitUntil(() => ancestors.length === 2);

		context.setSelection(['document-1', 'document-2', 'document-3']);
		await waitUntil(() => ancestors.length === 3);

		expect(ancestorRequests).to.deep.equal([['document-1', 'document-2'], ['document-3']]);
	});

	it('exposes the requested tree item of each selected document with its ancestors', async () => {
		context.setSelection(['document-1']);

		await waitUntil(() => ancestors.length === 1);
		expect(ancestors[0].treeItem).to.deep.equal({ unique: 'document-1', entityType: UMB_DOCUMENT_ENTITY_TYPE });
	});

	it('exposes null for a selected document that has no ancestors to return', async () => {
		itemsWithoutAncestors.add('document-2');

		context.setSelection(['document-1', 'document-2']);

		await waitUntil(() => ancestors.length === 2);
		expect(ancestors.find((entry) => entry.treeItem.unique === 'document-2')?.ancestors).to.be.null;
	});

	it('requests the ancestors again for a selected document that had none to return when the data type changes', async () => {
		const propertyContext = new UmbPropertyTypeBasedPropertyContext(hostElement);
		itemsWithoutAncestors.add('document-2');
		context.setSelection(['document-1', 'document-2']);
		await waitUntil(() => ancestors.length === 2);

		itemsWithoutAncestors.delete('document-2');
		propertyContext.setDataType({ unique: 'data-type-1' });

		await waitUntil(() => ancestorNamesOf('document-2')?.[0] === 'Folder B');
		expect(ancestorRequests[1]).to.have.members(['document-1', 'document-2']);
	});

	it('drops the ancestors of documents removed from the selection', async () => {
		context.setSelection(['document-1', 'document-2']);
		await waitUntil(() => ancestors.length === 2);

		context.setSelection(['document-2']);

		await waitUntil(() => ancestors.length === 1);
		expect(ancestors[0].treeItem.unique).to.equal('document-2');
	});

	it('requests the ancestors again for the documents below an entity whose structure changed', async () => {
		context.setSelection(['document-1', 'document-2', 'document-3']);
		await waitUntil(() => ancestors.length === 3);

		treeItemNames['folder-a'] = 'Renamed Folder';
		actionEventContext.dispatchEvent(
			new UmbRequestReloadStructureForEntityEvent({ unique: 'folder-a', entityType: UMB_DOCUMENT_ENTITY_TYPE }),
		);

		await waitUntil(() => ancestorNamesOf('document-1')?.[0] === 'Renamed Folder');
		await waitUntil(() => ancestorNamesOf('document-3')?.[0] === 'Renamed Folder');
		expect(ancestorRequests[1]).to.have.members(['document-1', 'document-3']);
		expect(ancestorNamesOf('document-2')).to.deep.equal(['Folder B']);
	});

	it('requests the ancestors again for a selected document whose structure changed', async () => {
		context.setSelection(['document-1', 'document-2']);
		await waitUntil(() => ancestors.length === 2);

		parentOf['document-2'] = 'folder-a';
		actionEventContext.dispatchEvent(
			new UmbRequestReloadStructureForEntityEvent({ unique: 'document-2', entityType: UMB_DOCUMENT_ENTITY_TYPE }),
		);

		await waitUntil(() => ancestorNamesOf('document-2')?.[0] === 'Folder A');
		expect(ancestorRequests[1]).to.deep.equal(['document-2']);
	});

	it('ignores structure changes of entities outside the selected structures', async () => {
		context.setSelection(['document-1']);
		await waitUntil(() => ancestors.length === 1);

		actionEventContext.dispatchEvent(
			new UmbRequestReloadStructureForEntityEvent({ unique: 'folder-b', entityType: UMB_DOCUMENT_ENTITY_TYPE }),
		);
		await new Promise((resolve) => setTimeout(resolve, 50));

		expect(ancestorRequests).to.have.lengthOf(1);
	});

	it('requests the ancestors for the data type of the property the picker is in', async () => {
		const propertyContext = new UmbPropertyTypeBasedPropertyContext(hostElement);
		propertyContext.setDataType({ unique: 'data-type-1' });

		context.setSelection(['document-1']);
		await waitUntil(() => ancestors.length === 1);

		expect(ancestorRequestDataTypeIds).to.deep.equal(['data-type-1']);
	});

	it('requests the ancestors again when the data type changes', async () => {
		const propertyContext = new UmbPropertyTypeBasedPropertyContext(hostElement);
		context.setSelection(['document-1']);
		await waitUntil(() => ancestors.length === 1);

		propertyContext.setDataType({ unique: 'data-type-1' });

		await waitUntil(() => ancestorRequests.length === 2);
		expect(ancestorRequests[1]).to.deep.equal(['document-1']);
		expect(ancestorRequestDataTypeIds.at(-1)).to.equal('data-type-1');
	});
});
