import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry, UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS, UmbWorkspaceElement } from '@umbraco-cms/backoffice/workspace';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { manifests as workspaceConditionManifests } from '../../../../core/workspace/conditions/manifests.js';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
const PARENT_ENTITY = { entityType: 'document', unique: null } as const;

// Everything the test extensions and the hooks do, in the order it happened.
let log: Array<string> = [];
const API_LOAD_DELAY = 300;
const wait = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

// A condition that answers according to the test, or never answers at all.
const conditionByKey = new Map<string, UmbTestCondition>();
interface TestConditionConfig extends UmbConditionConfigBase {
	key: string;
	initial?: boolean;
}
class UmbTestCondition extends UmbConditionBase<TestConditionConfig> {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<TestConditionConfig>) {
		super(host, args);
		conditionByKey.set(args.config.key, this);
		if (args.config.initial !== undefined) this.permitted = args.config.initial;
	}
	flipTo(value: boolean) {
		this.permitted = value;
	}
}
const TEST_CONDITION_ALIAS = 'Test.Condition.LoadingFlow';

// The workspace context that the extensions below take part in.
interface UmbTestExtensionOptions {
	name: string;
	registerLoadingHook?: boolean;
	incomingWeight?: number;
	throwOnIncomingData?: boolean;
}
function createTestExtensionClass(options: UmbTestExtensionOptions) {
	return class UmbTestWorkspaceContextExtension extends UmbControllerBase {
		constructor(host: UmbControllerHost, workspace: UmbDocumentWorkspaceContext) {
			super(host);
			log.push(`${options.name}:created`);

			if (options.registerLoadingHook) {
				workspace.loadingHook.add(() => {
					log.push(`${options.name}:loading`);
				});
			}
			workspace.incomingDataHook.add((data) => {
				if (options.throwOnIncomingData) throw new Error(`${options.name} failed`);
				log.push(`${options.name}:incoming`);
				return { ...data, isTrashed: true };
			}, options.incomingWeight ?? 0);
		}
	};
}

let resolveWorkspaceContext: (context: UmbDocumentWorkspaceContext) => void;

// Starts the flow in the same way the routing does: from the element that the workspace renders.
@customElement('umb-test-loading-flow-workspace-view')
class UmbTestLoadingFlowWorkspaceViewElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			if (context) resolveWorkspaceContext(context as UmbDocumentWorkspaceContext);
		});
	}
}

const WORKSPACE_ALIAS = 'Test.Workspace.LoadingFlow';
const EXTENSION_TYPE = 'workspaceContext';

describe('Document workspace loading flow, with extensions taking part in the loading hook', function () {
	this.timeout(6000);

	let hostElement: UmbTestDocumentWorkspaceHostElement;
	const registeredAliases: Array<string> = [];

	const registerExtension = (
		alias: string,
		options: UmbTestExtensionOptions,
		conditions?: Array<{ alias: string; [key: string]: unknown }>,
	) => {
		registeredAliases.push(alias);
		umbExtensionsRegistry.register({
			type: EXTENSION_TYPE,
			name: alias,
			alias,
			// The API loads slower than the mock request for the entity, so only waiting for it makes the extension take part.
			api: async () => {
				await wait(API_LOAD_DELAY);
				return { api: createTestExtensionClass(options) };
			},
			conditions,
		} as unknown as UmbExtensionManifest);
	};

	const openWorkspace = async () => {
		const renderedContext = new Promise<UmbDocumentWorkspaceContext>((resolve) => {
			resolveWorkspaceContext = resolve;
		});
		const workspaceElement = new UmbWorkspaceElement();
		hostElement.appendChild(workspaceElement);
		workspaceElement.entityType = 'document';
		return renderedContext;
	};

	before(() => {
		umbExtensionsRegistry.registerMany([
			...TEST_MANIFESTS,
			...workspaceConditionManifests,
			{ type: 'condition', name: 'Test Condition', alias: TEST_CONDITION_ALIAS, api: UmbTestCondition },
			{
				type: 'workspace',
				name: 'Test Workspace',
				alias: WORKSPACE_ALIAS,
				api: UmbDocumentWorkspaceContext,
				element: () => Promise.resolve({ default: UmbTestLoadingFlowWorkspaceViewElement }),
				meta: { entityType: 'document', headline: 'Test' },
			},
		] as unknown as Array<UmbExtensionManifest>);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany([
			...TEST_MANIFESTS.map((m) => m.alias),
			...workspaceConditionManifests.map((m) => m.alias),
			TEST_CONDITION_ALIAS,
			WORKSPACE_ALIAS,
		]);
	});

	beforeEach(async () => {
		log = [];
		conditionByKey.clear();
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
		umbExtensionsRegistry.unregisterMany(registeredAliases.splice(0));
	});

	describe('without any extensions', () => {
		it('loads without waiting for anything', async () => {
			const context = await openWorkspace();
			const start = performance.now();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(context.getData()?.unique).to.equal(INVARIANT_DOCUMENT_ID);
			expect(performance.now() - start).to.be.lessThan(1000);
		});
	});

	describe('with an extension that has no conditions', () => {
		beforeEach(() => {
			registerExtension('Test.Ext.A', { name: 'A' });
		});

		it('has the extension created before the incoming data hook runs', async () => {
			const context = await openWorkspace();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.deep.equal(['A:created', 'A:incoming']);
		});

		it('applies the data returned by the extension', async () => {
			const context = await openWorkspace();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(context.getData()?.isTrashed).to.be.true;
		});

		it('takes part when a document is created', async () => {
			const context = await openWorkspace();

			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);

			expect(log).to.deep.equal(['A:created', 'A:incoming']);
		});

		it('takes part in a reload, with the incoming data hook only', async () => {
			const context = await openWorkspace();
			await context.load(INVARIANT_DOCUMENT_ID);
			log = [];

			await context.reload();

			expect(log).to.deep.equal(['A:incoming']);
		});

		it('resolves isLoaded() only after the extension has taken part', async () => {
			const context = await openWorkspace();

			context.load(INVARIANT_DOCUMENT_ID);
			await context.isLoaded();

			expect(log).to.deep.equal(['A:created', 'A:incoming']);
			expect(context.getData()?.isTrashed).to.be.true;
		});
	});

	describe('with several extensions', () => {
		it('has all of them created before the first incoming data hook runs, and runs those by weight', async () => {
			registerExtension('Test.Ext.Low', { name: 'low', incomingWeight: 1 });
			registerExtension('Test.Ext.High', { name: 'high', incomingWeight: 10 });
			const context = await openWorkspace();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log.slice(0, 2).sort()).to.deep.equal(['high:created', 'low:created']);
			expect(log.slice(2)).to.deep.equal(['high:incoming', 'low:incoming']);
		});
	});

	describe('with an extension limited to new entities', () => {
		beforeEach(() => {
			registerExtension('Test.Ext.New', { name: 'new' }, [{ alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS }]);
		});

		it('takes part when creating, without waiting for the timeout', async () => {
			const context = await openWorkspace();
			const start = performance.now();

			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);

			expect(log).to.include('new:incoming');
			expect(performance.now() - start).to.be.lessThan(1500);
		});

		it('is skipped when loading, without waiting for the timeout', async () => {
			const context = await openWorkspace();
			const start = performance.now();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.not.include('new:incoming');
			expect(performance.now() - start).to.be.lessThan(1500);
		});
	});

	describe('with an extension whose condition says no', () => {
		it('does not wait for it, and it does not take part', async () => {
			registerExtension('Test.Ext.No', { name: 'no' }, [{ alias: TEST_CONDITION_ALIAS, key: 'no', initial: false }]);
			const context = await openWorkspace();
			const start = performance.now();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.deep.equal([]);
			expect(performance.now() - start).to.be.lessThan(1500);
		});
	});

	describe('with an extension whose condition never answers', () => {
		it('continues after the timeout, and names the extension in the warning', async () => {
			registerExtension('Test.Ext.Silent', { name: 'silent' }, [{ alias: TEST_CONDITION_ALIAS, key: 'silent' }]);
			const warnings: Array<unknown> = [];
			const originalWarn = console.warn;
			console.warn = (...args: Array<unknown>) => warnings.push(args);
			try {
				const context = await openWorkspace();

				await context.load(INVARIANT_DOCUMENT_ID);

				expect(context.getData()?.unique).to.equal(INVARIANT_DOCUMENT_ID);
				expect(JSON.stringify(warnings)).to.include('Test.Ext.Silent');
			} finally {
				console.warn = originalWarn;
			}
		});
	});

	describe('with an extension that is only permitted after the load', () => {
		it('does not delay the load, and misses that load', async () => {
			registerExtension('Test.Ext.Late', { name: 'late' }, [
				{ alias: TEST_CONDITION_ALIAS, key: 'late', initial: false },
			]);
			const context = await openWorkspace();
			const start = performance.now();

			await context.load(INVARIANT_DOCUMENT_ID);
			conditionByKey.get('late')!.flipTo(true);
			await wait(100);

			expect(log).to.not.include('late:incoming');
			expect(performance.now() - start).to.be.lessThan(1500);
		});

		it('takes part in a later reload', async () => {
			registerExtension('Test.Ext.Late', { name: 'late' }, [
				{ alias: TEST_CONDITION_ALIAS, key: 'late', initial: false },
			]);
			const context = await openWorkspace();
			await context.load(INVARIANT_DOCUMENT_ID);
			conditionByKey.get('late')!.flipTo(true);
			await wait(API_LOAD_DELAY + 200);

			await context.reload();

			expect(log).to.include('late:incoming');
		});
	});

	describe('with an extension whose incoming data hook throws', () => {
		it('fails the load, and the workspace is no longer loading', async () => {
			registerExtension('Test.Ext.Broken', { name: 'broken', throwOnIncomingData: true });
			const context = await openWorkspace();

			let error: unknown;
			await context.load(INVARIANT_DOCUMENT_ID).catch((e) => (error = e));
			await context.isLoaded();

			expect(error).to.be.instanceOf(Error);
			expect(context.getData()).to.be.undefined;
		});
	});

	describe('when the load starts as early as possible', () => {
		it('still waits for the extensions', async () => {
			registerExtension('Test.Ext.A', { name: 'A' });
			const workspaceElement = new UmbWorkspaceElement();
			hostElement.appendChild(workspaceElement);
			const consumer = new UmbTestLoadingFlowWorkspaceViewElement();
			const earliest = new Promise<UmbDocumentWorkspaceContext>((resolve) => {
				resolveWorkspaceContext = resolve;
			});
			workspaceElement.appendChild(consumer);
			workspaceElement.entityType = 'document';
			const context = await earliest;

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.deep.equal(['A:created', 'A:incoming']);
		});
	});

	describe('with an extension that adds a loading hook method of its own', () => {
		it('is too late for the loading hook that is already running, as the extension only exists because of it', async () => {
			registerExtension('Test.Ext.A', { name: 'A', registerLoadingHook: true });
			const context = await openWorkspace();

			await context.load(INVARIANT_DOCUMENT_ID);

			expect(log).to.deep.equal(['A:created', 'A:incoming']);
		});
	});
});
