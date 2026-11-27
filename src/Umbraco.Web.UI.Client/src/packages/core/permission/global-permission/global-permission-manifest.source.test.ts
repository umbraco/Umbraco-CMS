import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import { umbPermissionRegistry } from '../registry/permission.registry.js';
import { UmbTestPermissionHostElement, waitFor } from '../registry/permission.test-utils.js';
import type { UmbPermission } from '../types.js';
import type { ManifestGlobalPermission } from './global-permission.extension.js';
import { UmbGlobalPermissionManifestSource } from './global-permission-manifest.source.js';
import { expect } from '@open-wc/testing';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { Subscription } from '@umbraco-cms/backoffice/external/rxjs';

const READ = 'Test.Read';
const UPDATE = 'Test.Update';
const LOCKED = { title: 'Locked', detail: 'The thing is locked.' };
const TOGGLE_CONDITION_ALIAS = 'Test.Condition.PermissionManifestToggle';

const toggle = new UmbBooleanState(false);

class UmbTestToggleCondition extends UmbConditionBase<UmbConditionConfigBase> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);
		this.observe(toggle.asObservable(), (value) => (this.permitted = value), null);
	}
}

describe('UmbGlobalPermissionManifestSource', () => {
	let host: UmbTestPermissionHostElement;
	let aliases: Array<string>;
	let results: Array<Array<UmbPermission>>;
	let subscription: Subscription;

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			alias: TOGGLE_CONDITION_ALIAS,
			name: 'Test Toggle Condition',
			api: UmbTestToggleCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(TOGGLE_CONDITION_ALIAS);
	});

	beforeEach(() => {
		host = new UmbTestPermissionHostElement();
		aliases = [];
		results = [];
		toggle.setValue(false);
		subscription = umbPermissionRegistry
			.permissionsOfType(UMB_GLOBAL_PERMISSION_TYPE)
			.subscribe((permissions) => results.push(permissions));
	});

	afterEach(() => {
		subscription.unsubscribe();
		host.destroy();
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	function registerPermission(meta: ManifestGlobalPermission['meta'], conditions?: ManifestGlobalPermission['conditions']) {
		const alias = `Test.GlobalPermission.${aliases.length}`;
		umbExtensionsRegistry.register({ type: 'globalPermission', alias, name: alias, meta, conditions });
		aliases.push(alias);
		return alias;
	}

	it('sets allow, deny and denyReason as global permissions', async () => {
		registerPermission({ allow: [READ] });
		registerPermission({ deny: [UPDATE], denyReason: LOCKED });
		new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 2);

		expect(results.at(-1)).to.deep.equal([
			{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ], deny: undefined, denyReason: undefined },
			{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: undefined, deny: [UPDATE], denyReason: LOCKED },
		]);
	});

	it('makes the verbs of a manifest permitted', async () => {
		registerPermission({ allow: [READ] });
		new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 1);

		expect((await umbPermissionRegistry.getPermissionResult(host, { allOf: [READ] })).permitted).to.be.true;
	});

	it('adds a manifest that is registered later', async () => {
		new UmbGlobalPermissionManifestSource(host);
		registerPermission({ allow: [READ] });
		await waitFor(() => results.at(-1)?.length === 1);

		expect(results.at(-1)?.[0].allow).to.deep.equal([READ]);
	});

	it('leaves out a manifest whose conditions are not met, and adds it once they are', async () => {
		registerPermission({ allow: [READ] });
		registerPermission({ allow: [UPDATE] }, [{ alias: TOGGLE_CONDITION_ALIAS }]);
		new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 1);
		await new Promise((resolve) => setTimeout(resolve, 20));

		expect(results.at(-1)?.map((permission) => permission.allow)).to.deep.equal([[READ]]);

		toggle.setValue(true);
		await waitFor(() => results.at(-1)?.length === 2);

		expect(results.at(-1)?.map((permission) => permission.allow)).to.deep.equal([[READ], [UPDATE]]);
	});

	it('removes a manifest whose conditions are no longer met', async () => {
		toggle.setValue(true);
		registerPermission({ allow: [READ] }, [{ alias: TOGGLE_CONDITION_ALIAS }]);
		new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 1);
		toggle.setValue(false);
		await waitFor(() => results.at(-1)?.length === 0);

		expect(results.at(-1)).to.deep.equal([]);
	});

	it('removes the permissions of an unregistered manifest', async () => {
		registerPermission({ allow: [READ] });
		const alias = registerPermission({ allow: [UPDATE] });
		new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 2);
		umbExtensionsRegistry.unregister(alias);
		await waitFor(() => results.at(-1)?.length === 1);

		expect(results.at(-1)?.map((permission) => permission.allow)).to.deep.equal([[READ]]);
	});

	it('removes all of its permissions when destroyed', async () => {
		registerPermission({ allow: [READ] });
		const source = new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 1);
		source.destroy();

		expect(results.at(-1)).to.deep.equal([]);
	});

	it('keeps the permissions of other sources', async () => {
		const other = { type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] };
		umbPermissionRegistry.setSource('Test.Other', [other]);
		registerPermission({ allow: [READ] });
		const source = new UmbGlobalPermissionManifestSource(host);
		await waitFor(() => results.at(-1)?.length === 2);
		source.destroy();
		umbPermissionRegistry.removeSource('Test.Other');

		expect(results.at(-2)).to.deep.equal([other]);
	});
});
