import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS } from '../../user-permissions/document/conditions/constants.js';
import { expect, waitUntil } from '@open-wc/testing';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

const PARENT_ENTITY = { entityType: 'document', unique: null } as const;

let liveConditions = 0;

class UmbTestUserPermissionCondition extends UmbConditionBase<any> {
	#destroyed = false;

	constructor(host: any, args: any) {
		super(host, args);
		liveConditions++;
	}

	override destroy() {
		if (!this.#destroyed) {
			this.#destroyed = true;
			liveConditions--;
		}
		super.destroy();
	}
}

const MANIFESTS = TEST_MANIFESTS.filter((manifest) => manifest.alias !== UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS);

describe('UmbDocumentWorkspaceContext (user permission enforcement)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(MANIFESTS);
		umbExtensionsRegistry.register({
			type: 'condition',
			name: 'Test Document User Permission Condition',
			alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
			api: UmbTestUserPermissionCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS);
		umbExtensionsRegistry.unregisterMany(MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		liveConditions = 0;
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('keeps a single permission condition alive however often the workspace switches between new and existing', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);
		await waitUntil(() => liveConditions === 1);

		await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
		await context.load(VARIANT_DOCUMENT_ID);
		await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
		await context.load(INVARIANT_DOCUMENT_ID);

		await waitUntil(() => liveConditions === 1);
	});

	it('enforces the read-only rule again after the workspace state has been reset', async () => {
		// The test condition never reports a change, so the user is treated as lacking permission.
		await context.load(INVARIANT_DOCUMENT_ID);
		await waitUntil(() => liveConditions === 1);
		expect(await firstValueFrom(context.readOnlyGuard.hasRules)).to.be.true;

		await context.load(VARIANT_DOCUMENT_ID);
		await waitUntil(() => liveConditions === 1);

		expect(await firstValueFrom(context.readOnlyGuard.hasRules)).to.be.true;
	});
});
