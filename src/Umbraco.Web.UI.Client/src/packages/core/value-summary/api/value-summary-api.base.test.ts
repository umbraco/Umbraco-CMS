import { UmbValueSummaryApiBase } from './value-summary-api.base.js';
import { UmbValueSummaryCoordinatorContext } from '../coordinator/value-summary-coordinator.context.js';
import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-api-base-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValueSummaryApiBase', () => {
	let host: TestHostElement;

	beforeEach(() => {
		host = document.createElement('test-api-base-host') as TestHostElement;
		document.body.appendChild(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
		const registered = umbExtensionsRegistry.getByType('valueSummary');
		for (const ext of registered) {
			if (ext.alias.startsWith('Umb.Test.')) {
				umbExtensionsRegistry.unregister(ext.alias);
			}
		}
	});

	it('should expose a value observable', () => {
		const api = new UmbValueSummaryApiBase(host);
		expect(api).to.have.property('value').that.is.instanceOf(Observable);
		api.destroy();
	});

	it('should pass raw value through when no coordinator is available', async () => {
		const api = new UmbValueSummaryApiBase(host);
		api.valueType = 'Umb.Test.NoCoordinator';
		api.rawValue = 'hello';

		// Wait for microtask debounce
		await new Promise((r) => queueMicrotask(() => r(undefined)));

		const value = await new Promise<unknown>((resolve) => {
			api.value.subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});
		expect(value).to.equal('hello');
		api.destroy();
	});

	it('should not connect until valueType is set', async () => {
		const api = new UmbValueSummaryApiBase(host);
		api.rawValue = 'test';

		// Wait for microtask
		await new Promise((r) => queueMicrotask(() => r(undefined)));

		let received = false;
		api.value.subscribe((v) => {
			if (v !== undefined) received = true;
		});

		// Wait another microtask
		await new Promise((r) => queueMicrotask(() => r(undefined)));
		expect(received).to.be.false;
		api.destroy();
	});

	it('should connect to coordinator and receive resolved values', async () => {
		const valueType = 'Umb.Test.ApiBaseCoord';
		const manifest: ManifestValueSummary = {
			type: 'valueSummary',
			alias: `Umb.Test.ValueSummary.${valueType}`,
			name: `Test Value Summary ${valueType}`,
			forValueType: valueType,
			element: () => import('../boolean/boolean-value-summary.element.js'),
			meta: {},
		} as unknown as ManifestValueSummary;

		(manifest as any).resolver = {
			api: class {
				resolveValues = async (values: ReadonlyArray<unknown>) => values.map((v) => `resolved:${v}`);
				destroy() {}
			},
		};

		umbExtensionsRegistry.register(manifest);

		// Create coordinator as parent context
		const coordinator = new UmbValueSummaryCoordinatorContext(host);

		// Create a child element for the API
		const child = document.createElement('test-api-base-host') as TestHostElement;
		host.appendChild(child);

		const api = new UmbValueSummaryApiBase(child);
		api.valueType = valueType;
		api.rawValue = 'test';

		const value = await new Promise<unknown>((resolve) => {
			api.value.subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});

		expect(value).to.equal('resolved:test');
		api.destroy();
		coordinator.destroy();
	});

	it('should debounce connect via microtask when setting valueType and rawValue', async () => {
		let connectCalls = 0;
		const valueType = 'Umb.Test.Debounce';

		const manifest: ManifestValueSummary = {
			type: 'valueSummary',
			alias: `Umb.Test.ValueSummary.${valueType}`,
			name: `Test Value Summary ${valueType}`,
			forValueType: valueType,
			element: () => import('../boolean/boolean-value-summary.element.js'),
			meta: {},
		} as unknown as ManifestValueSummary;

		(manifest as any).resolver = {
			api: class {
				resolveValues = async (values: ReadonlyArray<unknown>) => {
					connectCalls++;
					return values.map((v) => `r:${v}`);
				};
				destroy() {}
			},
		};

		umbExtensionsRegistry.register(manifest);

		const coordinator = new UmbValueSummaryCoordinatorContext(host);

		const child = document.createElement('test-api-base-host') as TestHostElement;
		host.appendChild(child);

		const api = new UmbValueSummaryApiBase(child);

		// Set both properties in same tick
		api.valueType = valueType;
		api.rawValue = 'val';

		const value = await new Promise<unknown>((resolve) => {
			api.value.subscribe((v) => {
				if (v !== undefined) resolve(v);
			});
		});

		expect(value).to.equal('r:val');
		// The resolver should only have been called once (batched)
		expect(connectCalls).to.equal(1);

		api.destroy();
		coordinator.destroy();
	});
});
