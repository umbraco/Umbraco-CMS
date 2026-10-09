import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from '../conditions/constants.js';
import { UmbPropertyValueUserPermissionWorkspaceContextBase } from './property-value-user-permission-workspace-context-base.js';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

@customElement('umb-test-property-value-user-permission-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

let liveConditions = 0;

class UmbTestPermissionCondition extends UmbConditionBase<any> {
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

class UmbTestWorkspaceContext extends UmbPropertyValueUserPermissionWorkspaceContextBase {
	setPermissions(
		properties: Array<UmbPropertyTypeModel>,
		viewGuard: UmbVariantPropertyGuardManager,
		writeGuard: UmbVariantPropertyGuardManager,
	) {
		this._setPermissions(properties, viewGuard, writeGuard);
	}
}

const createProperties = (count: number) =>
	Array.from({ length: count }, (_, index) => ({ unique: `property-${index}` }) as UmbPropertyTypeModel);

const settle = () => new Promise((resolve) => setTimeout(resolve, 0));

describe('UmbPropertyValueUserPermissionWorkspaceContextBase', () => {
	let host: UmbTestControllerHostElement;
	let context: UmbTestWorkspaceContext;
	let viewGuard: UmbVariantPropertyGuardManager;
	let writeGuard: UmbVariantPropertyGuardManager;

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			name: 'Test Property Value User Permission Condition',
			alias: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS,
			api: UmbTestPermissionCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS);
	});

	beforeEach(() => {
		liveConditions = 0;
		host = new UmbTestControllerHostElement();
		context = new UmbTestWorkspaceContext(host);
		viewGuard = new UmbVariantPropertyGuardManager(host);
		writeGuard = new UmbVariantPropertyGuardManager(host);
	});

	it('keeps one read and one write condition per property', async () => {
		context.setPermissions(createProperties(3), viewGuard, writeGuard);
		await settle();

		expect(liveConditions).to.equal(6);
	});

	it('does not accumulate conditions when permissions are set repeatedly', async () => {
		context.setPermissions(createProperties(2), viewGuard, writeGuard);
		context.setPermissions(createProperties(8), viewGuard, writeGuard);
		context.setPermissions(createProperties(9), viewGuard, writeGuard);
		await settle();

		expect(liveConditions).to.equal(18);
	});

	it('only replaces conditions of the guards it is given again', async () => {
		const otherViewGuard = new UmbVariantPropertyGuardManager(host);
		const otherWriteGuard = new UmbVariantPropertyGuardManager(host);

		context.setPermissions(createProperties(2), viewGuard, writeGuard);
		context.setPermissions(createProperties(3), otherViewGuard, otherWriteGuard);
		context.setPermissions(createProperties(1), viewGuard, writeGuard);
		await settle();

		expect(liveConditions).to.equal(2 + 6);
	});
});
