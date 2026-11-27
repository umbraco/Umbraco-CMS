import type { ManifestPermissionTypeEvaluator } from '../evaluator/permission-type-evaluator.extension.js';
import { UmbPermissionTypeEvaluatorController } from './permission-type-evaluator.controller.js';
import {
	createTestEvaluatorClass,
	UmbTestPermissionHostElement,
	waitFor,
	withoutConsoleError,
} from './permission.test-utils.js';
import { expect } from '@open-wc/testing';

const READ = 'Test.Read';
const UPDATE = 'Test.Update';

describe('UmbPermissionTypeEvaluatorController', () => {
	let host: UmbTestPermissionHostElement;
	let evaluator: ReturnType<typeof createTestEvaluatorClass>;
	let manifest: ManifestPermissionTypeEvaluator;

	beforeEach(() => {
		host = new UmbTestPermissionHostElement();
		evaluator = createTestEvaluatorClass();
		manifest = {
			type: 'permissionTypeEvaluator',
			alias: 'Test.PermissionTypeEvaluator',
			name: 'Test Permission Type Evaluator',
			forPermissionType: 'test-type',
			forPermissionVerbs: [READ, UPDATE],
			api: evaluator,
		};
	});

	afterEach(() => {
		host.destroy();
	});

	it('creates the evaluator on the controller', async () => {
		const controller = new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		await waitFor(() => evaluator.instances.length === 1);

		expect(controller.hasUmbController(evaluator.instances[0])).to.be.true;
	});

	it('asks the evaluator for the verdicts of the given verbs', async () => {
		new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);

		expect(evaluator.instances[0].askedVerbs).to.deep.equal([READ]);
	});

	it('gives the evaluator the permissions set before it was created', async () => {
		const permission = { type: 'test-type', allow: [READ] };
		const controller = new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		controller.setPermissions([permission]);
		await waitFor(() => evaluator.instances[0]?.askedVerbs !== undefined);

		expect(evaluator.instances[0].receivedPermissions).to.deep.equal([[permission]]);
		expect(evaluator.instances[0].permissionsWhenAsked).to.deep.equal([permission]);
	});

	it('gives the evaluator permissions set later', async () => {
		const permission = { type: 'test-type', deny: [READ] };
		const controller = new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		await waitFor(() => evaluator.instances.length === 1);
		controller.setPermissions([permission]);

		expect(evaluator.instances[0].receivedPermissions.at(-1)).to.deep.equal([permission]);
	});

	it('holds the verdicts, undefined until the evaluator has answered', async () => {
		const controller = new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		const results: Array<unknown> = [];
		controller.verdicts.subscribe((verdicts) => results.push(verdicts));
		await waitFor(() => evaluator.instances.length === 1);
		evaluator.instances[0].verdictsState.setValue({ [READ]: { permitted: true } });

		expect(results).to.deep.equal([undefined, { [READ]: { permitted: true } }]);
	});

	it('destroys the evaluator with the controller', async () => {
		const controller = new UmbPermissionTypeEvaluatorController(host, manifest, [READ]);
		await waitFor(() => evaluator.instances.length === 1);
		controller.destroy();

		expect(evaluator.instances[0].destroyed).to.be.true;
	});

	it('destroys an evaluator that is created after the controller was destroyed, and does not use it', async () => {
		const rejections: Array<unknown> = [];
		const onRejection = (event: PromiseRejectionEvent) => {
			rejections.push(event.reason);
			event.preventDefault();
		};
		window.addEventListener('unhandledrejection', onRejection);

		try {
			let resolveApi: () => void = () => {};
			const slowManifest: ManifestPermissionTypeEvaluator = {
				...manifest,
				api: () => new Promise((resolve) => (resolveApi = () => resolve({ default: evaluator }))),
			};
			const controller = new UmbPermissionTypeEvaluatorController(host, slowManifest, [READ]);
			controller.destroy();
			resolveApi();
			await waitFor(() => evaluator.instances.length === 1);
			await new Promise((resolve) => setTimeout(resolve, 20));
		} finally {
			window.removeEventListener('unhandledrejection', onRejection);
		}

		expect(evaluator.instances[0].destroyed).to.be.true;
		expect(evaluator.instances[0].askedVerbs).to.be.undefined;
		expect(rejections).to.deep.equal([]);
	});

	it('denies the given verbs when the evaluator cannot be created', async () => {
		const brokenManifest: ManifestPermissionTypeEvaluator = {
			...manifest,
			api: () => Promise.reject(new Error('Not found')),
		};

		await withoutConsoleError(async () => {
			const controller = new UmbPermissionTypeEvaluatorController(host, brokenManifest, [READ, UPDATE]);
			let verdicts: unknown;
			controller.verdicts.subscribe((value) => (verdicts = value));
			await waitFor(() => verdicts !== undefined);

			expect(verdicts).to.deep.equal({ [READ]: { permitted: false }, [UPDATE]: { permitted: false } });
		});
	});
});
