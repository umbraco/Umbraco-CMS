import { UmbDisplayBatchResolverCoordinatorContext } from './display-batch-resolver-coordinator.context.js';
import type { UmbDisplayBatchResolveResult, UmbDisplayBatchResolverLoaderProperty } from './display-batch-resolver.interface.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { BehaviorSubject, map } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-display-batch-resolver-coordinator-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const TEST_TOKEN_A = new UmbContextToken<UmbDisplayBatchResolverCoordinatorContext>('UmbTestDisplayBatchResolverCoordinatorA');
const TEST_TOKEN_B = new UmbContextToken<UmbDisplayBatchResolverCoordinatorContext>('UmbTestDisplayBatchResolverCoordinatorB');

function makeResolver(
	resolveValues: (values: ReadonlyArray<unknown>) => Promise<UmbDisplayBatchResolveResult>,
): UmbDisplayBatchResolverLoaderProperty {
	return class {
		resolveValues = resolveValues;
		destroy() {}
	};
}

function waitForValue(coordinator: UmbDisplayBatchResolverCoordinatorContext, key: string, rawValue: unknown, expected: unknown) {
	return new Promise<void>((resolve) => {
		coordinator.observeResolvedValue(key, rawValue).subscribe((v) => {
			if (v === expected) resolve();
		});
	});
}

describe('UmbDisplayBatchResolverCoordinatorContext', () => {
	let host: TestHostElement;

	beforeEach(() => {
		host = document.createElement('test-display-batch-resolver-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('resolves values with the resolver the lookup returns for their key', async () => {
		const lookedUpKeys: Array<string> = [];
		const resolver = makeResolver(async (values) => ({ data: values.map((v) => `resolved:${v}`) }));
		const coordinator = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_A, (key) => {
			lookedUpKeys.push(key);
			return key === 'with-resolver' ? resolver : undefined;
		});

		coordinator.register('with-resolver', 'a');
		await waitForValue(coordinator, 'with-resolver', 'a', 'resolved:a');

		expect(lookedUpKeys).to.include('with-resolver');
		coordinator.destroy();
	});

	it('passes values through unchanged when the lookup returns no resolver', async () => {
		const coordinator = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_A, () => undefined);

		coordinator.register('no-resolver', 'raw');
		await waitForValue(coordinator, 'no-resolver', 'raw', 'raw');

		coordinator.destroy();
	});

	it('provides itself under the context token it is constructed with', async () => {
		const coordinator = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_A, () => undefined);
		const child = document.createElement('test-display-batch-resolver-coordinator-host') as TestHostElement;
		host.appendChild(child);

		const consumed = await new Promise<UmbDisplayBatchResolverCoordinatorContext | undefined>((resolve) => {
			new UmbContextConsumerController(child, TEST_TOKEN_A, resolve);
		});

		expect(consumed).to.equal(coordinator);
		coordinator.destroy();
	});

	it('keeps instances independent, so the same key can resolve differently per owner', async () => {
		const coordinatorA = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_A, () =>
			makeResolver(async (values) => ({ data: values.map((v) => `A:${v}`) })),
		);
		const coordinatorB = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_B, () =>
			makeResolver(async (values) => ({ data: values.map((v) => `B:${v}`) })),
		);

		coordinatorA.register('shared-key', 'x');
		coordinatorB.register('shared-key', 'x');

		await Promise.all([
			waitForValue(coordinatorA, 'shared-key', 'x', 'A:x'),
			waitForValue(coordinatorB, 'shared-key', 'x', 'B:x'),
		]);

		coordinatorA.destroy();
		coordinatorB.destroy();
	});

	it('keeps values from every flush following resolver updates', async () => {
		const source = new BehaviorSubject<Record<string, string>>({ a: 'A', b: 'B' });
		const resolver = makeResolver(async (values) => ({
			data: values.map((v) => source.getValue()[v as string]),
			asObservable: () => source.pipe(map((names) => values.map((v) => names[v as string]))),
		}));
		const coordinator = new UmbDisplayBatchResolverCoordinatorContext(host, TEST_TOKEN_A, () => resolver);

		coordinator.register('key', 'a');
		await waitForValue(coordinator, 'key', 'a', 'A');

		coordinator.register('key', 'b');
		await waitForValue(coordinator, 'key', 'b', 'B');

		source.next({ a: 'A renamed', b: 'B renamed' });

		let resolvedA: unknown;
		let resolvedB: unknown;
		coordinator.observeResolvedValue('key', 'a').subscribe((v) => (resolvedA = v));
		coordinator.observeResolvedValue('key', 'b').subscribe((v) => (resolvedB = v));

		expect(resolvedA).to.equal('A renamed');
		expect(resolvedB).to.equal('B renamed');
		coordinator.destroy();
	});
});
