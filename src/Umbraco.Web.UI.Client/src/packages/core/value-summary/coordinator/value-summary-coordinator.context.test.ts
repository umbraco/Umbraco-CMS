import { UmbValueSummaryCoordinatorContext } from './value-summary-coordinator.context.js';
import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-coordinator-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

function makeManifest(
	valueType: string,
	resolverFn?: (values: ReadonlyArray<unknown>) => Promise<{ data: ReadonlyArray<unknown> }>,
): ManifestValueSummary {
	const manifest: ManifestValueSummary = {
		type: 'valueSummary',
		alias: `Umb.Test.ValueSummary.${valueType}`,
		name: `Test Value Summary ${valueType}`,
		forValueType: valueType,
		element: () => import('../value-types/boolean/boolean-value-summary.element.js'),
		meta: {},
	} as unknown as ManifestValueSummary;

	if (resolverFn) {
		(manifest as any).valueResolver = class {
			resolveValues = resolverFn;
			destroy() {}
		};
	}

	return manifest;
}

describe('UmbValueSummaryCoordinatorContext', () => {
	let host: TestHostElement;
	let coordinator: UmbValueSummaryCoordinatorContext;

	beforeEach(() => {
		host = document.createElement('test-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
		coordinator = new UmbValueSummaryCoordinatorContext(host);
	});

	afterEach(() => {
		coordinator.destroy();
		document.body.innerHTML = '';
		const registered = umbExtensionsRegistry.getByType('valueSummary');
		for (const ext of registered) {
			if (ext.alias.startsWith('Umb.Test.')) {
				umbExtensionsRegistry.unregister(ext.alias);
			}
		}
	});

	it('should be created', () => {
		expect(coordinator).to.be.instanceOf(UmbValueSummaryCoordinatorContext);
	});

	it('should pass raw values through when no resolver is registered', async () => {
		const valueType = 'Umb.Test.PassThrough';
		const manifest = makeManifest(valueType);
		umbExtensionsRegistry.register(manifest);

		coordinator.preRegister(valueType, 'hello');
		const obs = coordinator.observeResolvedValue(valueType, 'hello');
		expect(obs).to.be.instanceOf(Observable);

		const value = await new Promise<unknown>((resolve) => {
			obs.subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});
		expect(value).to.equal('hello');
	});

	it('should pass raw values through when no manifest is registered', async () => {
		const valueType = 'Umb.Test.NoManifest';
		coordinator.preRegister(valueType, 42);

		const value = await new Promise<unknown>((resolve) => {
			coordinator.observeResolvedValue(valueType, 42).subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});
		expect(value).to.equal(42);
	});

	it('should resolve values using the resolver', async () => {
		const valueType = 'Umb.Test.Resolver';
		const manifest = makeManifest(valueType, async (values) => ({ data: values.map((v) => `resolved:${v}`) }));
		umbExtensionsRegistry.register(manifest);

		coordinator.preRegister(valueType, 'a');

		const value = await new Promise<unknown>((resolve) => {
			coordinator.observeResolvedValue(valueType, 'a').subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});
		expect(value).to.equal('resolved:a');
	});

	it('should batch multiple preRegister calls into a single resolver call', async () => {
		const valueType = 'Umb.Test.Batch';
		let callCount = 0;
		const manifest = makeManifest(valueType, async (values) => {
			callCount++;
			return { data: values.map((v) => `resolved:${v}`) };
		});
		umbExtensionsRegistry.register(manifest);

		coordinator.preRegister(valueType, 'x');
		coordinator.preRegister(valueType, 'y');

		const [vx, vy] = await Promise.all([
			new Promise<unknown>((resolve) => {
				coordinator.observeResolvedValue(valueType, 'x').subscribe((v) => {
					if (v !== undefined) resolve(v);
				});
			}),
			new Promise<unknown>((resolve) => {
				coordinator.observeResolvedValue(valueType, 'y').subscribe((v) => {
					if (v !== undefined) resolve(v);
				});
			}),
		]);

		expect(vx).to.equal('resolved:x');
		expect(vy).to.equal('resolved:y');
		expect(callCount).to.equal(1);
	});

	it('should maintain positional mapping for resolved values', async () => {
		const valueType = 'Umb.Test.Positional';
		const manifest = makeManifest(valueType, async (values) => ({
			data: values.map((v) => ({ original: v, doubled: `${v}${v}` })),
		}));
		umbExtensionsRegistry.register(manifest);

		coordinator.preRegister(valueType, 'a');
		coordinator.preRegister(valueType, 'b');

		const [va, vb] = await Promise.all([
			new Promise<unknown>((resolve) => {
				coordinator.observeResolvedValue(valueType, 'a').subscribe((v) => {
					if (v !== undefined) resolve(v);
				});
			}),
			new Promise<unknown>((resolve) => {
				coordinator.observeResolvedValue(valueType, 'b').subscribe((v) => {
					if (v !== undefined) resolve(v);
				});
			}),
		]);

		expect(va).to.deep.equal({ original: 'a', doubled: 'aa' });
		expect(vb).to.deep.equal({ original: 'b', doubled: 'bb' });
	});

	it('should reuse resolver instances across multiple flushes', async () => {
		const valueType = 'Umb.Test.Reuse';
		let constructCount = 0;

		const manifest: ManifestValueSummary = {
			type: 'valueSummary',
			alias: `Umb.Test.ValueSummary.${valueType}`,
			name: `Test Value Summary ${valueType}`,
			forValueType: valueType,
			element: () => import('../value-types/boolean/boolean-value-summary.element.js'),
			meta: {},
		} as unknown as ManifestValueSummary;

		(manifest as any).valueResolver = class {
			constructor() {
				constructCount++;
			}
			resolveValues = async (values: ReadonlyArray<unknown>) => ({ data: values.map((v) => `r:${v}`) });
			destroy() {}
		};

		umbExtensionsRegistry.register(manifest);

		// First flush
		coordinator.preRegister(valueType, 'a');
		await new Promise<void>((resolve) => {
			coordinator.observeResolvedValue(valueType, 'a').subscribe((v) => {
				if (v !== undefined) resolve();
			});
		});

		// Second flush (new tick)
		coordinator.preRegister(valueType, 'b');
		await new Promise<void>((resolve) => {
			coordinator.observeResolvedValue(valueType, 'b').subscribe((v) => {
				if (v !== undefined) resolve();
			});
		});

		expect(constructCount).to.equal(1);
	});
});
