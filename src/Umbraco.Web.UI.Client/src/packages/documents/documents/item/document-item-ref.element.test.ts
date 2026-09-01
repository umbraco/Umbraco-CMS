import { UmbDocumentItemRefElement } from './document-item-ref.element.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbDocumentVariantState } from '../variant-state.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

// Stands in for a non-routable host, such as a confirm dialog, where no route context is available.
@customElement('umb-test-document-item-ref-host')
class UmbTestDocumentItemRefHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentItemRefElement', () => {
	let hostElement: UmbTestDocumentItemRefHostElement;
	let element: UmbDocumentItemRefElement;

	beforeEach(async () => {
		hostElement = new UmbTestDocumentItemRefHostElement();
		document.body.appendChild(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		element = document.createElement('umb-document-item-ref');
		element.item = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: 'test-123',
			documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
			hasChildren: false,
			isProtected: false,
			isTrashed: false,
			parent: null,
			flags: [],
			variants: [{ culture: 'en-US', name: 'English Title', state: UmbDocumentVariantState.PUBLISHED, flags: [] }],
		};
		hostElement.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDocumentItemRefElement);
	});

	it('links to the document workspace when no route context is available', async () => {
		await waitUntil(
			() => !!element.shadowRoot?.querySelector('uui-ref-node')?.getAttribute('href'),
			'expected the ref node to get an href without a route context',
		);

		const refNode = element.shadowRoot!.querySelector('uui-ref-node')!;
		expect(refNode.getAttribute('href')).to.equal(
			UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: 'test-123' }),
		);
		expect(refNode.getAttribute('target')).to.equal('_blank');
	});
});
