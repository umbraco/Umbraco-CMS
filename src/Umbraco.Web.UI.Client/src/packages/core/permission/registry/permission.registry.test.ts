import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import type { UmbPermissionConfig, UmbPermissionResult } from '../types.js';
import { UmbPermissionRegistry } from './permission.registry.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-permission-registry-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const READ = 'Test.Read';
const UPDATE = 'Test.Update';
const DELETE = 'Test.Delete';
const LOCKED = { title: 'Locked', detail: 'The thing is locked.' };

describe('UmbPermissionRegistry', () => {
	let host: UmbTestControllerHostElement;
	let registry: UmbPermissionRegistry;

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		registry = new UmbPermissionRegistry();
	});

	function getResult(config: UmbPermissionConfig) {
		return registry.getPermissionResult(host, config);
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
			registry.setSource('Test.Source', [{ type: 'test-other', allow: [READ] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
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
		it('returns the reasons of the denies that blocked a verb, once each', async () => {
			const otherReason = { title: 'Other' };
			registry.setSource('Test.Source', [
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ, UPDATE], denyReason: LOCKED },
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE], denyReason: otherReason },
			]);

			expect((await getResult({ allOf: [READ, UPDATE] })).denyReasons).to.deep.equal([LOCKED, otherReason]);
		});

		it('returns no reasons when the result is permitted', async () => {
			registry.setSource('Test.Source', [
				{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] },
				{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [UPDATE], denyReason: LOCKED },
			]);

			expect(await getResult({ oneOf: [READ, UPDATE] })).to.deep.equal({ permitted: true, denyReasons: [] });
		});
	});

	describe('sources', () => {
		it('replaces the permissions of a source when it is set again', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
			expect((await getResult({ allOf: [UPDATE] })).permitted).to.be.true;
		});

		it('removes the permissions of a source', async () => {
			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			registry.removeSource('Test.Source');

			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
		});

		it('ignores permissions without a type and warns', async () => {
			const warnings: Array<unknown> = [];
			const originalWarn = console.warn;
			console.warn = (...args: Array<unknown>) => warnings.push(args);

			try {
				// @ts-expect-error - JavaScript and JSON callers can leave out the type.
				registry.setSource('Test.Source', [{ allow: [READ] }]);
			} finally {
				console.warn = originalWarn;
			}

			expect(warnings).to.have.lengthOf(1);
			expect((await getResult({ allOf: [READ] })).permitted).to.be.false;
		});
	});

	describe('permissionResult', () => {
		it('updates when the permissions change', () => {
			const results: Array<UmbPermissionResult> = [];
			const subscription = registry
				.permissionResult(host, { allOf: [READ] })
				.subscribe((result) => results.push(result));

			registry.setSource('Test.Allow', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [READ] }]);
			registry.setSource('Test.Deny', [{ type: UMB_GLOBAL_PERMISSION_TYPE, deny: [READ], denyReason: LOCKED }]);
			registry.removeSource('Test.Deny');
			subscription.unsubscribe();

			expect(results).to.deep.equal([
				{ permitted: false, denyReasons: [] },
				{ permitted: true, denyReasons: [] },
				{ permitted: false, denyReasons: [LOCKED] },
				{ permitted: true, denyReasons: [] },
			]);
		});

		it('does not emit when a change leaves the result the same', () => {
			const results: Array<UmbPermissionResult> = [];
			const subscription = registry
				.permissionResult(host, { allOf: [READ] })
				.subscribe((result) => results.push(result));

			registry.setSource('Test.Source', [{ type: UMB_GLOBAL_PERMISSION_TYPE, allow: [UPDATE] }]);
			subscription.unsubscribe();

			expect(results).to.have.lengthOf(1);
		});
	});
});
