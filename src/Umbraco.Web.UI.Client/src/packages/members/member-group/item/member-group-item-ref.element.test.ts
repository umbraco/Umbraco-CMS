import { UmbMemberGroupItemRefElement } from './member-group-item-ref.element.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// Stands in for a non-routable host, such as a confirm dialog, where no route context is available.
@customElement('umb-test-member-group-item-ref-host')
class UmbTestMemberGroupItemRefHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Stands in for the real section permission condition, so the tests can decide whether the current
// user reaches the Member Management section without standing up the whole current-user stack.
let userHasSectionAccess = true;

class UmbTestSectionUserPermissionCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: { config: UmbConditionConfigBase; onChange: () => void }) {
		super(host, args);
		this.permitted = userHasSectionAccess;
	}
}

describe('UmbMemberGroupItemRefElement', () => {
	let hostElement: UmbTestMemberGroupItemRefHostElement;

	async function renderRef() {
		const element = document.createElement('umb-member-group-item-ref');
		element.item = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: 'test-123',
			name: 'Editors',
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

	beforeEach(() => {
		userHasSectionAccess = true;
		hostElement = new UmbTestMemberGroupItemRefHostElement();
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', async () => {
		expect(await renderRef()).to.be.instanceOf(UmbMemberGroupItemRefElement);
	});

	it('links to the member group workspace when no route context is available', async () => {
		const element = await renderRef();

		await waitUntil(
			() => !!element.shadowRoot?.querySelector('uui-ref-node')?.getAttribute('href'),
			'expected the ref node to get an href without a route context',
		);

		const refNode = element.shadowRoot!.querySelector('uui-ref-node')!;
		expect(refNode.getAttribute('href')).to.equal(
			UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: 'test-123' }),
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
