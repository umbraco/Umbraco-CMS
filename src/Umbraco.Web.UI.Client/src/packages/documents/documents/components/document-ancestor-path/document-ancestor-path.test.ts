import { UmbDocumentAncestorPathElement } from './document-ancestor-path.element.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentVariantState } from '../../variant-state.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

@customElement('umb-test-document-ancestor-path-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const contentType = { unique: 'dt-1', icon: 'icon-folder', collection: null };

const createAncestor = (unique: string, name: string): UmbDocumentTreeItemModel => ({
	entityType: UMB_DOCUMENT_ENTITY_TYPE,
	unique,
	name,
	parent: { entityType: 'document-root', unique: null },
	ancestors: [],
	hasChildren: true,
	isFolder: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	flags: [],
	contentType,
	documentType: contentType,
	createDate: '2024-01-01T00:00:00Z',
	variants: [{ culture: null, segment: null, name, state: UmbDocumentVariantState.PUBLISHED, flags: [] }],
});

describe('UmbDocumentAncestorPathElement', () => {
	let hostElement: UmbTestHostElement;
	let element: UmbDocumentAncestorPathElement;

	const getPath = () => element.shadowRoot?.textContent?.trim();

	beforeEach(async () => {
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		element = document.createElement('umb-document-ancestor-path');
		hostElement.appendChild(element);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('renders the ancestor names from the root down', async () => {
		element.ancestors = [createAncestor('root', 'Home'), createAncestor('folder', 'People')];

		await waitUntil(() => getPath() === 'Home / People', 'expected the ancestor names joined from the root down');
	});

	it('renders the new path when the ancestors change', async () => {
		element.ancestors = [createAncestor('root', 'Home')];
		await waitUntil(() => getPath() === 'Home', 'expected the first path');

		element.ancestors = [createAncestor('root', 'Home'), createAncestor('folder', 'Archive')];

		await waitUntil(() => getPath() === 'Home / Archive', 'expected the updated path');
	});

	it('renders nothing when there are no ancestors', async () => {
		element.ancestors = [createAncestor('root', 'Home')];
		await waitUntil(() => getPath() === 'Home', 'expected the first path');

		element.ancestors = [];
		await element.updateComplete;

		expect(getPath()).to.equal('');
	});
});
