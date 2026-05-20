import { expect } from '@open-wc/testing';
import {
	umbExtension,
	getExtensionManifest,
	registerExtensionModule,
	unregisterExtensionModule,
} from './umb-extension.decorator.js';
import type { ManifestBase } from '../types/index.js';

describe('umbExtension decorator', () => {
	it('stores manifest metadata on the decorated class', () => {
		const manifest = { type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' };

		@umbExtension(manifest)
		class TestDashboard {}

		const stored = getExtensionManifest(TestDashboard);
		expect(stored).to.deep.equal(manifest);
	});

	it('returns undefined for undecorated classes', () => {
		class PlainClass {}
		expect(getExtensionManifest(PlainClass)).to.be.undefined;
	});

	it('returns the decorated class from the decorator', () => {
		const manifest = { type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' };

		@umbExtension(manifest)
		class TestDashboard {}

		expect(TestDashboard).to.be.a('function');
		expect(getExtensionManifest(TestDashboard)).to.not.be.undefined;
	});

	it('preserves additional manifest properties', () => {
		const manifest = {
			type: 'entityAction',
			alias: 'Test.Action',
			name: 'Test',
			forEntityTypes: ['document'],
			meta: { label: 'Do it', icon: 'icon-wand' },
			conditions: [{ alias: 'Umb.Condition.SectionAlias', match: 'Umb.Section.Content' }],
		};

		@umbExtension(manifest)
		class TestAction {}

		const stored = getExtensionManifest(TestAction);
		expect(stored).to.deep.equal(manifest);
	});
});

describe('registerExtensionModule', () => {
	let registry: { register: (manifest: ManifestBase) => void; registered: ManifestBase[] };

	beforeEach(() => {
		registry = {
			registered: [],
			register(manifest: ManifestBase) {
				this.registered.push(manifest);
			},
		};
	});

	it('registers an HTMLElement subclass as element', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class TestDashboard extends HTMLElement {}

		registerExtensionModule({ TestDashboard }, registry);

		expect(registry.registered).to.have.length(1);
		expect(registry.registered[0]).to.have.property('element', TestDashboard);
		expect(registry.registered[0]).to.not.have.property('api');
	});

	it('registers a non-HTMLElement class as api', () => {
		@umbExtension({ type: 'entityAction', alias: 'Test.Action', name: 'Test' })
		class TestAction {
			execute() {}
		}

		registerExtensionModule({ TestAction }, registry);

		expect(registry.registered).to.have.length(1);
		expect(registry.registered[0]).to.have.property('api', TestAction);
		expect(registry.registered[0]).to.not.have.property('element');
	});

	it('preserves explicit api reference from manifest', () => {
		class MyApi {
			execute() {}
		}

		// Using 'as any' because ManifestBase doesn't have 'api' — specific types like ManifestElementAndApi do
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		@umbExtension({ type: 'entityAction', alias: 'Test.Action', name: 'Test', api: MyApi } as any)
		class TestElement extends HTMLElement {}

		registerExtensionModule({ TestElement }, registry);

		expect(registry.registered).to.have.length(1);
		expect(registry.registered[0]).to.have.property('element', TestElement);
		expect(registry.registered[0]).to.have.property('api', MyApi);
	});

	it('registers multiple decorated classes from one module', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.DashA', name: 'Dashboard A' })
		class DashboardA extends HTMLElement {}

		@umbExtension({ type: 'dashboard', alias: 'Test.DashB', name: 'Dashboard B' })
		class DashboardB extends HTMLElement {}

		registerExtensionModule({ DashboardA, DashboardB }, registry);

		expect(registry.registered).to.have.length(2);
		expect(registry.registered[0]).to.have.property('alias', 'Test.DashA');
		expect(registry.registered[0]).to.have.property('element', DashboardA);
		expect(registry.registered[1]).to.have.property('alias', 'Test.DashB');
		expect(registry.registered[1]).to.have.property('element', DashboardB);
	});

	it('registers mixed element and api classes from one module', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Dashboard' })
		class MyDashboard extends HTMLElement {}

		@umbExtension({ type: 'entityAction', alias: 'Test.Action', name: 'Action' })
		class MyAction {
			execute() {}
		}

		registerExtensionModule({ MyDashboard, MyAction }, registry);

		expect(registry.registered).to.have.length(2);
		const dashboard = registry.registered.find((m: any) => m.alias === 'Test.Dashboard')!;
		const action = registry.registered.find((m: any) => m.alias === 'Test.Action')!;
		expect(dashboard).to.have.property('element', MyDashboard);
		expect(action).to.have.property('api', MyAction);
	});

	it('skips undecorated exports', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class TestDashboard extends HTMLElement {}

		class NotDecorated extends HTMLElement {}

		registerExtensionModule({ TestDashboard, NotDecorated }, registry);

		expect(registry.registered).to.have.length(1);
		expect(registry.registered[0]).to.have.property('alias', 'Test.Dashboard');
	});

	it('returns true when decorated classes are found', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class TestDashboard extends HTMLElement {}

		const result = registerExtensionModule({ TestDashboard }, registry);
		expect(result).to.be.true;
	});

	it('returns false when no decorated classes are found', () => {
		class PlainClass {}
		const result = registerExtensionModule({ PlainClass }, registry);
		expect(result).to.be.false;
	});

	it('returns false for empty module exports', () => {
		const result = registerExtensionModule({}, registry);
		expect(result).to.be.false;
	});
});

describe('unregisterExtensionModule', () => {
	let unregistered: string[];
	let unregistry: { unregister: (alias: string) => void };

	beforeEach(() => {
		unregistered = [];
		unregistry = {
			unregister(alias: string) {
				unregistered.push(alias);
			},
		};
	});

	it('unregisters decorated classes by alias', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class TestDashboard extends HTMLElement {}

		unregisterExtensionModule({ TestDashboard }, unregistry);

		expect(unregistered).to.have.length(1);
		expect(unregistered[0]).to.equal('Test.Dashboard');
	});

	it('unregisters multiple decorated classes', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.DashA', name: 'A' })
		class DashA extends HTMLElement {}

		@umbExtension({ type: 'dashboard', alias: 'Test.DashB', name: 'B' })
		class DashB extends HTMLElement {}

		unregisterExtensionModule({ DashA, DashB }, unregistry);

		expect(unregistered).to.have.length(2);
		expect(unregistered).to.include('Test.DashA');
		expect(unregistered).to.include('Test.DashB');
	});

	it('skips undecorated exports', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class Decorated extends HTMLElement {}

		class NotDecorated extends HTMLElement {}

		unregisterExtensionModule({ Decorated, NotDecorated }, unregistry);

		expect(unregistered).to.have.length(1);
		expect(unregistered[0]).to.equal('Test.Dashboard');
	});

	it('returns true when decorated classes are found', () => {
		@umbExtension({ type: 'dashboard', alias: 'Test.Dashboard', name: 'Test' })
		class TestDashboard extends HTMLElement {}

		const result = unregisterExtensionModule({ TestDashboard }, unregistry);
		expect(result).to.be.true;
	});

	it('returns false when no decorated classes are found', () => {
		class PlainClass {}
		const result = unregisterExtensionModule({ PlainClass }, unregistry);
		expect(result).to.be.false;
	});

	it('returns false for empty module exports', () => {
		const result = unregisterExtensionModule({}, unregistry);
		expect(result).to.be.false;
	});
});
