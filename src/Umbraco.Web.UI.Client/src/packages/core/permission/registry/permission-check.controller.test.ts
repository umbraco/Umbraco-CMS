import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import type { UmbPermissionConfig, UmbPermissionResult } from '../types.js';
import { UmbPermissionCheckController } from './permission-check.controller.js';
import { UmbPermissionRegistry } from './permission.registry.js';
import {
	createTestEvaluatorClass,
	UmbTestPermissionHostElement,
	waitFor,
	withoutConsoleError,
} from './permission.test-utils.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { Subscription } from '@umbraco-cms/backoffice/external/rxjs';

const READ = 'Test.Read';
const UPDATE = 'Test.Update';
const DELETE = 'Test.Delete';
const TEST_TYPE = 'test-type';
const OTHER_TYPE = 'test-other-type';
const LOCKED = { title: 'Locked', detail: 'The thing is locked.' };

describe('UmbPermissionCheckController', () => {
	let host: UmbTestPermissionHostElement;
	let registry: UmbPermissionRegistry;
	let aliases: Array<string>;
	let subscriptions: Array<Subscription>;

	beforeEach(() => {
		host = new UmbTestPermissionHostElement();
		registry = new UmbPermissionRegistry();
		aliases = [];
		subscriptions = [];
	});

	afterEach(() => {
		subscriptions.forEach((subscription) => subscription.unsubscribe());
		host.destroy();
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	function registerEvaluator(forPermissionVerbs: Array<string>, forPermissionType = TEST_TYPE) {
		const api = createTestEvaluatorClass();
		const alias = `Test.PermissionTypeEvaluator.${aliases.length}`;
		umbExtensionsRegistry.register({
			type: 'permissionTypeEvaluator',
			alias,
			name: alias,
			forPermissionType,
			forPermissionVerbs,
			api,
		});
		aliases.push(alias);
		return api;
	}

	function createCheck(config: UmbPermissionConfig) {
		return new UmbPermissionCheckController(host, registry, config);
	}

	function getResult(config: UmbPermissionConfig) {
		return createCheck(config).getDeterminedResult();
	}

	function observeResults(config: UmbPermissionConfig) {
		const results: Array<UmbPermissionResult> = [];
		subscriptions.push(createCheck(config).result.subscribe((result) => results.push(result)));
		return results;
	}

	describe('global permissions', () => {
		it('denies by default', async () => {
			expect(await getResult({ allOf: [READ] })).to.deep.equal({ permitted: false, denyReasons: [] });
		});

		it('permits an allowed verb', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

			expect(await getResult({ allOf: [READ] })).to.deep.equal({ permitted: true, denyReasons: [] });
		});

		it('lets a deny win over an allow from another source', async () => {
			registry.setSource('Test.Allow', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			registry.setSource('Test.Deny', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ], denyReason: LOCKED }]);

			expect(await getResult({ allOf: [READ] })).to.deep.equal({ permitted: false, denyReasons: [LOCKED] });
		});

		it('lets a deny win over an allow in the same permission', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ], deny: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
		});

		it('ignores permissions of other types', async () => {
			registry.setSource('Test.Source', [{ type: OTHER_TYPE, allow: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
		});

		it('updates the result when the permissions change', () => {
			const results = observeResults({ allOf: [READ] });

			registry.setSource('Test.Allow', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			registry.setSource('Test.Deny', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ], denyReason: LOCKED }]);
			registry.removeSource('Test.Deny');

			expect(results).to.deep.equal([
				{ permitted: false, denyReasons: [] },
				{ permitted: true, denyReasons: [] },
				{ permitted: false, denyReasons: [LOCKED] },
				{ permitted: true, denyReasons: [] },
			]);
		});

		it('does not emit when a change leaves the result the same', () => {
			const results = observeResults({ allOf: [READ] });

			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);

			expect(results).to.have.lengthOf(1);
		});
	});

	describe('config', () => {
		beforeEach(() => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ, UPDATE] }]);
		});

		const cases: Array<{ config: UmbPermissionConfig; expected: boolean }> = [
			{ config: {}, expected: false },
			{ config: { allOf: [], oneOf: [] }, expected: false },
			{ config: { allOf: [READ, UPDATE] }, expected: true },
			{ config: { allOf: [READ, DELETE] }, expected: false },
			{ config: { oneOf: [DELETE, UPDATE] }, expected: true },
			{ config: { oneOf: [DELETE] }, expected: false },
			{ config: { allOf: [READ], oneOf: [DELETE, UPDATE] }, expected: true },
			{ config: { allOf: [READ], oneOf: [DELETE] }, expected: false },
			{ config: { allOf: [DELETE], oneOf: [READ] }, expected: false },
		];

		for (const { config, expected } of cases) {
			it(`is ${expected ? '' : 'not '}permitted for ${JSON.stringify(config)} when Read and Update are allowed`, async () => {
				expect((await getResult(config)).permitted).to.equal(expected);
			});
		}
	});

	describe('deny reasons', () => {
		it('returns the reasons of the global denies that blocked a verb, once each', async () => {
			const otherReason = { title: 'Other' };
			registry.setSource('Test.Source', [
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ, UPDATE], denyReason: LOCKED },
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE], denyReason: otherReason },
			]);

			expect((await getResult({ allOf: [READ, UPDATE] })).denyReasons).to.deep.equal([LOCKED, otherReason]);
		});

		it('returns no reason for a global deny without one', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ] }]);

			expect(await getResult({ allOf: [READ] })).to.deep.equal({ permitted: false, denyReasons: [] });
		});

		it('returns the reasons of the denying verdicts, once each', async () => {
			const evaluator = registerEvaluator([READ, UPDATE]);
			const result = getResult({ allOf: [READ, UPDATE] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({
				[READ]: { permitted: false, denyReasons: [LOCKED] },
				[UPDATE]: { permitted: false, denyReasons: [LOCKED] },
			});

			expect((await result).denyReasons).to.deep.equal([LOCKED]);
		});

		it('returns no reasons for a denying verdict without any', async () => {
			const evaluator = registerEvaluator([READ]);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(await result).to.deep.equal({ permitted: false, denyReasons: [] });
		});

		it('returns no reasons when the result is permitted', async () => {
			registry.setSource('Test.Source', [
				{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] },
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE], denyReason: LOCKED },
			]);

			expect(await getResult({ oneOf: [READ, UPDATE] })).to.deep.equal({ permitted: true, denyReasons: [] });
		});

		it('returns no reasons while the result is undetermined', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE], denyReason: LOCKED }]);
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ oneOf: [READ, UPDATE] });
			await waitFor(() => evaluator.instances.length === 1);

			expect(results.at(-1)).to.deep.equal({ permitted: undefined, denyReasons: [] });
		});
	});

	describe('verdicts', () => {
		it('grants a verb an evaluator permits', async () => {
			const evaluator = registerEvaluator([READ]);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });

			expect(await result).to.deep.equal({ permitted: true, denyReasons: [] });
		});

		it('denies a verb an evaluator denies, even when a global permission allows it', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			const evaluator = registerEvaluator([READ]);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false, denyReasons: [LOCKED] } });

			expect(await result).to.deep.equal({ permitted: false, denyReasons: [LOCKED] });
		});

		it('requires every evaluator with a verdict to permit', async () => {
			const first = registerEvaluator([READ]);
			const second = registerEvaluator([READ], OTHER_TYPE);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => first.instances.length === 1 && second.instances.length === 1);
			first.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });
			second.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect((await result).permitted).to.be.false;
		});

		it('lets a global deny win over a permitting verdict', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ], denyReason: LOCKED }]);
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });

			expect(results.at(-1)).to.deep.equal({ permitted: false, denyReasons: [LOCKED] });
		});

		it('falls back to global permissions when no evaluator has a verdict', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			const evaluator = registerEvaluator([READ, UPDATE]);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({});

			expect((await result).permitted).to.be.true;
		});

		it('denies by default when no evaluator has a verdict and no global permission allows the verb', async () => {
			const evaluator = registerEvaluator([READ]);
			const result = getResult({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({});

			expect(await result).to.deep.equal({ permitted: false, denyReasons: [] });
		});

		it('ignores verdicts for verbs the evaluator does not list', async () => {
			const evaluator = registerEvaluator([READ]);
			const result = getResult({ allOf: [READ, DELETE] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true }, [DELETE]: { permitted: true } });

			expect((await result).permitted).to.be.false;
		});

		it('updates the result when a verdict changes', async () => {
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(results.map((result) => result.permitted)).to.deep.equal([undefined, true, false]);
		});
	});

	describe('undetermined', () => {
		it('is undefined until every evaluator involved has answered', async () => {
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);

			expect(results.at(-1)?.permitted).to.be.undefined;

			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });

			expect(results.at(-1)?.permitted).to.be.true;
		});

		it('is denied as soon as an evaluator denies, before others have answered', async () => {
			const first = registerEvaluator([READ]);
			const second = registerEvaluator([READ], OTHER_TYPE);
			const results = observeResults({ allOf: [READ] });
			await waitFor(() => first.instances.length === 1 && second.instances.length === 1);
			first.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(results.at(-1)?.permitted).to.be.false;
		});

		it('is denied as soon as one verb of allOf is denied, while another is undetermined', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE] }]);
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ allOf: [READ, UPDATE] });
			await waitFor(() => evaluator.instances.length === 1);

			expect(results.at(-1)?.permitted).to.be.false;
		});

		it('is permitted as soon as one verb of oneOf is permitted, while another is undetermined', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ oneOf: [READ, UPDATE] });
			await waitFor(() => evaluator.instances.length === 1);

			expect(results.at(-1)?.permitted).to.be.true;
		});

		it('is undefined while no verb of oneOf is permitted and one is undetermined', async () => {
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ oneOf: [READ, UPDATE] });
			await waitFor(() => evaluator.instances.length === 1);

			expect(results.at(-1)?.permitted).to.be.undefined;

			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(results.at(-1)?.permitted).to.be.false;
		});
	});

	describe('getDeterminedResult', () => {
		it('resolves with the first determined result', async () => {
			const evaluator = registerEvaluator([READ]);
			let resolved: UmbPermissionResult | undefined;
			createCheck({ allOf: [READ] })
				.getDeterminedResult()
				.then((result) => (resolved = result));
			await waitFor(() => evaluator.instances.length === 1);

			expect(resolved).to.be.undefined;

			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });
			await waitFor(() => resolved !== undefined);

			expect(resolved?.permitted).to.be.true;
		});

		it('resolves at once when the result is already determined', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.true;
		});
	});

	describe('evaluators', () => {
		it('creates an evaluator for each manifest that lists a requested verb, on the check', async () => {
			const first = registerEvaluator([READ]);
			const second = registerEvaluator([READ, UPDATE], OTHER_TYPE);
			observeResults({ allOf: [READ] });
			await waitFor(() => first.instances.length === 1 && second.instances.length === 1);

			expect(first.instances[0].getHostElement()).to.equal(host);
			expect(second.instances[0].getHostElement()).to.equal(host);
		});

		it('never creates an evaluator that lists none of the requested verbs', async () => {
			const evaluator = registerEvaluator([UPDATE]);
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.true;
			expect(evaluator.instances).to.have.lengthOf(0);
		});

		it('asks an evaluator only for the requested verbs it lists', async () => {
			const evaluator = registerEvaluator([READ, UPDATE]);
			observeResults({ allOf: [READ, DELETE] });
			await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);

			expect(evaluator.instances[0].askedVerbs).to.deep.equal([READ]);
		});

		it('asks for a verb once when the config requests it more than once', async () => {
			const evaluator = registerEvaluator([READ]);
			observeResults({ allOf: [READ, READ], oneOf: [READ] });
			await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);

			expect(evaluator.instances[0].askedVerbs).to.deep.equal([READ]);
		});

		it('gives an evaluator the permissions of its type before asking for verdicts', async () => {
			const ownPermission = { type: TEST_TYPE, allow: [READ] };
			registry.setSource('Test.Source', [
				ownPermission,
				{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] },
				{ type: OTHER_TYPE, allow: [READ] },
			]);
			const evaluator = registerEvaluator([READ]);
			observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);

			expect(evaluator.instances[0].permissionsWhenAsked).to.deep.equal([ownPermission]);
		});

		it('gives an evaluator its permissions again when they change, and not when other types change', async () => {
			const evaluator = registerEvaluator([READ]);
			observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);
			const permission = { type: TEST_TYPE, deny: [READ] };
			registry.setSource('Test.Source', [permission]);
			registry.setSource('Test.Other', [{ type: OTHER_TYPE, allow: [READ] }]);

			expect(evaluator.instances[0].receivedPermissions).to.deep.equal([[], [permission]]);
		});

		it('creates an evaluator that is registered after the check started', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			const results = observeResults({ allOf: [READ] });

			expect(results.at(-1)?.permitted).to.be.true;

			const evaluator = registerEvaluator([READ]);
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(results.at(-1)?.permitted).to.be.false;
		});

		it('keeps existing evaluators when another evaluator is registered', async () => {
			const first = registerEvaluator([READ]);
			observeResults({ allOf: [READ] });
			await waitFor(() => first.instances.length === 1);
			const second = registerEvaluator([READ], OTHER_TYPE);
			await waitFor(() => second.instances.length === 1);

			expect(first.instances).to.have.lengthOf(1);
			expect(first.instances[0].destroyed).to.be.false;
		});

		it('destroys the evaluator of an unregistered manifest and decides without it', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			const evaluator = registerEvaluator([READ]);
			const results = observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: false } });

			expect(results.at(-1)?.permitted).to.be.false;

			umbExtensionsRegistry.unregister(aliases[0]);

			expect(evaluator.instances[0].destroyed).to.be.true;
			expect(results.at(-1)?.permitted).to.be.true;
		});

		it('stops giving permissions to the evaluator of an unregistered manifest', async () => {
			const evaluator = registerEvaluator([READ]);
			observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);
			umbExtensionsRegistry.unregister(aliases[0]);
			registry.setSource('Test.Source', [{ type: TEST_TYPE, deny: [READ] }]);

			expect(evaluator.instances[0].receivedPermissions).to.deep.equal([[]]);
		});

		it('creates the evaluator again when its manifest is registered again', async () => {
			const evaluator = registerEvaluator([READ]);
			const manifest = umbExtensionsRegistry.getByAlias(aliases[0])!;
			observeResults({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			umbExtensionsRegistry.unregister(aliases[0]);
			umbExtensionsRegistry.register(manifest);
			await waitFor(() => evaluator.instances.length === 2);

			expect(evaluator.instances[0].destroyed).to.be.true;
			expect(evaluator.instances[1].destroyed).to.be.false;
		});

		it('denies the verbs of an evaluator that cannot be created', async () => {
			registry.setSource('Test.Global', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			const alias = 'Test.PermissionTypeEvaluator.Broken';
			umbExtensionsRegistry.register({
				type: 'permissionTypeEvaluator',
				alias,
				name: alias,
				forPermissionType: TEST_TYPE,
				forPermissionVerbs: [READ],
				api: () => Promise.reject(new Error('Not found')),
			});
			aliases.push(alias);

			await withoutConsoleError(async () => {
				expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
			});
		});

		it('ignores permissions of a type no evaluator reads', async () => {
			registry.setSource('Test.Source', [{ type: 'test-unread-type', allow: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
		});
	});

	describe('destroy', () => {
		it('destroys its evaluators', async () => {
			const evaluator = registerEvaluator([READ]);
			const check = createCheck({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			check.destroy();

			expect(evaluator.instances[0].destroyed).to.be.true;
		});

		it('is destroyed with its host', async () => {
			const evaluator = registerEvaluator([READ]);
			createCheck({ allOf: [READ] });
			await waitFor(() => evaluator.instances.length === 1);
			host.destroy();

			expect(evaluator.instances[0].destroyed).to.be.true;
		});

		it('stops creating evaluators', async () => {
			const check = createCheck({ allOf: [READ] });
			check.destroy();
			const evaluator = registerEvaluator([READ]);
			await new Promise((resolve) => setTimeout(resolve, 20));

			expect(evaluator.instances).to.have.lengthOf(0);
		});
	});
});
