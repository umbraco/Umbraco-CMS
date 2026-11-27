import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import { umbPermissionRegistry } from '../registry/permission.registry.js';
import {
	createTestEvaluatorClass,
	UmbTestPermissionHostElement,
	waitFor,
} from '../registry/permission.test-utils.js';
import type { UmbPermissionConfig } from '../types.js';
import { UMB_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbPermissionCondition } from './permission.condition.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const READ = 'Test.Read';
const UPDATE = 'Test.Update';
const SOURCE = 'Test.PermissionCondition';
const EVALUATOR_ALIAS = 'Test.PermissionCondition.Evaluator';

describe('UmbPermissionCondition', () => {
	let host: UmbTestPermissionHostElement;

	beforeEach(() => {
		host = new UmbTestPermissionHostElement();
	});

	afterEach(() => {
		host.destroy();
		umbPermissionRegistry.removeSource(SOURCE);
		umbExtensionsRegistry.unregister(EVALUATOR_ALIAS);
	});

	function createCondition(config: UmbPermissionConfig) {
		const changes: Array<boolean> = [];
		const condition = new UmbPermissionCondition(host, {
			host,
			config: { alias: UMB_PERMISSION_CONDITION_ALIAS, ...config },
			onChange: (permitted: boolean) => changes.push(permitted),
		});
		return { condition, changes };
	}

	it('is permitted when the verbs are permitted', () => {
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

		expect(createCondition({ allOf: [READ] }).condition.permitted).to.be.true;
	});

	it('is not permitted when the verbs are denied', () => {
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ], deny: [READ] }]);

		expect(createCondition({ allOf: [READ] }).condition.permitted).to.be.false;
	});

	it('requires every verb of allOf', () => {
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

		expect(createCondition({ allOf: [READ, UPDATE] }).condition.permitted).to.be.false;
	});

	it('requires one verb of oneOf', () => {
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);

		expect(createCondition({ oneOf: [READ, UPDATE] }).condition.permitted).to.be.true;
	});

	it('is not permitted without verbs', () => {
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

		expect(createCondition({}).condition.permitted).to.be.false;
	});

	it('updates when the permissions change', () => {
		const { condition, changes } = createCondition({ allOf: [READ] });
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
		umbPermissionRegistry.removeSource(SOURCE);

		expect(changes).to.deep.equal([true, false]);
		expect(condition.permitted).to.be.false;
	});

	it('is not permitted until the result is determined', async () => {
		const evaluator = createTestEvaluatorClass();
		umbExtensionsRegistry.register({
			type: 'permissionTypeEvaluator',
			alias: EVALUATOR_ALIAS,
			name: EVALUATOR_ALIAS,
			forPermissionType: 'test-type',
			forPermissionVerbs: [READ],
			api: evaluator,
		});
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);
		const { condition, changes } = createCondition({ allOf: [READ, UPDATE] });
		await waitFor(() => evaluator.instances.length === 1);

		expect(condition.permitted).to.be.false;
		expect(changes).to.deep.equal([]);

		evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });

		expect(condition.permitted).to.be.true;
	});

	it('stops updating when destroyed', () => {
		const { condition, changes } = createCondition({ allOf: [READ] });
		condition.destroy();
		umbPermissionRegistry.setSource(SOURCE, [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

		expect(changes).to.deep.equal([]);
	});
});
