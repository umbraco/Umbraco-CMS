import { UmbDocumentItemRefElement } from './document-item-ref.element.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbDocumentVariantState } from '../variant-state.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

// Stands in for a non-routable host, such as a confirm dialog, where no route context is available.
@customElement('umb-test-document-item-ref-host')
class UmbTestDocumentItemRefHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Stands in for the real section permission condition, so the tests can decide whether the current
// user reaches the Content section without standing up the whole current-user stack.
let userHasSectionAccess = true;

class UmbTestSectionUserPermissionCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: { config: UmbConditionConfigBase; onChange: () => void }) {
		super(host, args);
		this.permitted = userHasSectionAccess;
	}
}

describe('UmbDocumentItemRefElement', () => {
	let hostElement: UmbTestDocumentItemRefHostElement;

	async function renderRef() {
		const element = document.createElement('umb-document-item-ref');
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
		return element;
	}

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			name: 'Test Section User Permission Condition',
			alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
			api: UmbTestSectionUserPermissionCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS);
	});

	beforeEach(async () => {
		userHasSectionAccess = true;
		hostElement = new UmbTestDocumentItemRefHostElement();
		document.body.appendChild(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', async () => {
		expect(await renderRef()).to.be.instanceOf(UmbDocumentItemRefElement);
	});

	it('links to the document workspace when no route context is available', async () => {
		const element = await renderRef();

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

	it('is not readonly when the user has access to the section', async () => {
		const element = await renderRef();

		await waitUntil(
			() => element.shadowRoot?.querySelector('uui-ref-node')?.hasAttribute('readonly') === false,
			'expected the ref node not to be readonly',
		);
	});

	it('is readonly when the user has no access to the section', async () => {
		userHasSectionAccess = false;
		const element = await renderRef();

		expect(element.shadowRoot?.querySelector('uui-ref-node')?.hasAttribute('readonly')).to.be.true;
	});
});
