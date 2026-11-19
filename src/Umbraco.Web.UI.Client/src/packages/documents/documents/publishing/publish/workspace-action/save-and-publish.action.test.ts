import { UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS } from '../../../user-permissions/document/conditions/constants.js';
import type { UmbDocumentUserPermissionConditionConfig } from '../../../user-permissions/document/conditions/types.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../../user-permissions/document/constants.js';
import { UmbDocumentSaveAndPublishWorkspaceAction } from './save-and-publish.action.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-save-and-publish-action-host')
class UmbTestSaveAndPublishActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Replaces the document user permission condition, the way an extension can swap it out under the same alias.
class UmbTestDocumentUserPermissionCondition extends UmbConditionBase<UmbDocumentUserPermissionConditionConfig> {
	static instances: Array<UmbTestDocumentUserPermissionCondition> = [];
	destroyed = false;

	constructor(
		host: UmbControllerHost,
		args: { config: UmbDocumentUserPermissionConditionConfig; onChange: (permitted: boolean) => void },
	) {
		super(host, args);
		UmbTestDocumentUserPermissionCondition.instances.push(this);
	}

	override destroy() {
		this.destroyed = true;
		super.destroy();
	}
}

describe('UmbDocumentSaveAndPublishWorkspaceAction', () => {
	let host: UmbTestSaveAndPublishActionHostElement;
	let action: UmbDocumentSaveAndPublishWorkspaceAction;

	const isDisabled = () => firstValueFrom(action.isDisabled);

	async function getCondition() {
		await waitUntil(
			() => UmbTestDocumentUserPermissionCondition.instances.length > 0,
			'the registered document user permission condition was not used',
		);
		return UmbTestDocumentUserPermissionCondition.instances[0];
	}

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			name: 'Test Document User Permission Condition',
			alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
			api: UmbTestDocumentUserPermissionCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS);
	});

	beforeEach(() => {
		UmbTestDocumentUserPermissionCondition.instances = [];
		host = new UmbTestSaveAndPublishActionHostElement();
		action = new UmbDocumentSaveAndPublishWorkspaceAction(host, { meta: {} as never });
	});

	afterEach(() => {
		action.destroy();
	});

	it('is disabled until the permission condition permits it', async () => {
		expect(await isDisabled()).to.be.true;
	});

	it('requires both the update and publish permissions from the registered condition', async () => {
		const condition = await getCondition();

		expect(condition.config.allOf).to.deep.equal([
			UMB_USER_PERMISSION_DOCUMENT_UPDATE,
			UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		]);
	});

	it('is enabled when the registered condition permits it', async () => {
		const condition = await getCondition();

		condition.permitted = true;

		expect(await isDisabled()).to.be.false;
	});

	it('is disabled again when the registered condition no longer permits it', async () => {
		const condition = await getCondition();

		condition.permitted = true;
		condition.permitted = false;

		expect(await isDisabled()).to.be.true;
	});

	it('destroys the permission condition together with the action', async () => {
		const condition = await getCondition();

		action.destroy();

		expect(condition.destroyed).to.be.true;
	});
});
