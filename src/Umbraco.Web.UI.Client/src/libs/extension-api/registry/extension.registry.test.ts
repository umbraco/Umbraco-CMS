import type { WorkspaceAliasConditionConfig } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestElementWithElementName,
	ManifestKind,
	ManifestBase,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import { UmbExtensionRegistry } from './extension.registry.js';
import { expect } from '@open-wc/testing';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

interface TestManifestWithMeta extends ManifestBase {
	meta: unknown;
}

describe('UmbExtensionRegistry', () => {
	let extensionRegistry: UmbExtensionRegistry<TestManifestWithMeta>;
	let manifests: Array<TestManifestWithMeta>;

	beforeEach(() => {
		extensionRegistry = new UmbExtensionRegistry();
		manifests = [
			{
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					label: 'Test Section 1',
					pathname: 'test-section-1',
				},
			},
			{
				type: 'section',
				name: 'test-section-2',
				alias: 'Umb.Test.Section.2',
				weight: 200,
				meta: {
					label: 'Test Section 2',
					pathname: 'test-section-2',
				},
			},
			{
				type: 'section',
				name: 'test-section-3',
				alias: 'Umb.Test.Section.3',
				weight: 25,
				meta: {
					label: 'Test Section 3',
					pathname: 'test-section-3',
				},
			},
			{
				type: 'workspace',
				name: 'test-editor-1',
				alias: 'Umb.Test.Editor.1',
				weight: 2,
				meta: {
					entityType: 'testEntity',
				},
			},
		];

		manifests.forEach((manifest) => extensionRegistry.register(manifest));
	});

	afterEach(() => {
		extensionRegistry.clear();
	});

	it('should register an extension', () => {
		const registeredExtensions = extensionRegistry['_extensions'].getValue();
		expect(registeredExtensions).to.have.lengthOf(4);
		expect(registeredExtensions?.[0]?.alias).to.eq('Umb.Test.Section.1');
	});

	it('should say that an extension is registered', () => {
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.true;
	});

	it('should get several extensions by type', (done) => {
		extensionRegistry
			.byType('section')
			.subscribe((extensions) => {
				expect(extensions.length).to.eq(3);
				done();
			})
			.unsubscribe();
	});

	it('should get an extension by alias', (done) => {
		const alias = 'Umb.Test.Section.1';
		extensionRegistry
			.byAlias(alias)
			.subscribe((extension) => {
				expect(extension?.alias).to.eq(alias);
				done();
			})
			.unsubscribe();
	});

	it('should get an extension by type and alias', (done) => {
		const alias = 'Umb.Test.Section.1';
		extensionRegistry
			.byTypeAndAlias('section', alias)
			.subscribe((extension) => {
				expect(extension?.alias).to.eq(alias);
				done();
			})
			.unsubscribe();
	});

	it('should get an extension by type and filter', (done) => {
		extensionRegistry
			.byTypeAndFilter('section', (ext) => ext.weight === 25)
			.subscribe((extensions) => {
				expect(extensions.length).to.eq(1);
				expect(extensions[0].alias).to.eq('Umb.Test.Section.3');
				done();
			})
			.unsubscribe();
	});

	it('should get an extension by aliases', (done) => {
		const aliases = ['Umb.Test.Section.1', 'Umb.Test.Section.2'];
		extensionRegistry
			.byTypeAndAliases('section', aliases)
			.subscribe((extensions) => {
				expect(extensions[0]?.alias).to.eq(aliases[1]);
				expect(extensions[1]?.alias).to.eq(aliases[0]);
				done();
			})
			.unsubscribe();
	});

	describe('byType', () => {
		const type = 'section';

		it('should get all extensions by type', (done) => {
			extensionRegistry
				.byType(type)
				.subscribe((extensions) => {
					expect(extensions).to.have.lengthOf(3);
					expect(extensions?.[0]?.type).to.eq(type);
					expect(extensions?.[1]?.type).to.eq(type);
					done();
				})
				.unsubscribe();
		});

		it('should return extensions ordered by weight', (done) => {
			extensionRegistry
				.byType(type)
				.subscribe((extensions) => {
					expect(extensions?.[0]?.weight).to.eq(200);
					expect(extensions?.[1]?.weight).to.eq(25);
					expect(extensions?.[2]?.weight).to.eq(1);
					done();
				})
				.unsubscribe();
		});

		it('should only trigger observable when changes made to the scope of it', (done) => {
			let amountOfTimesTriggered = -1;
			let lastAmount = 0;

			extensionRegistry
				.byType('section')
				.subscribe((extensions) => {
					amountOfTimesTriggered++;
					const newAmount = extensions?.length ?? 0;
					if (amountOfTimesTriggered === 0) {
						expect(newAmount).to.eq(3);
					}
					if (amountOfTimesTriggered === 1) {
						expect(newAmount).to.eq(4);
					}
					if (lastAmount === newAmount) {
						expect(null).to.eq('Update was triggered without a change, this test should fail.');
					} else {
						lastAmount = newAmount;

						if (lastAmount === 3) {
							// We registerer a extension that should not affect this observable.
							extensionRegistry.register({
								type: 'workspace',
								name: 'test-editor-2',
								alias: 'Umb.Test.Editor.2',
								meta: {
									entityType: 'testEntity',
								},
							});
							// And then register a extension that triggers this observable.
							extensionRegistry.register({
								type: 'section',
								name: 'test-section-4',
								alias: 'Umb.Test.Section.4',
								weight: 9999,
								meta: {
									label: 'Test Section 4',
									pathname: 'test-section-4',
								},
							});
						}

						if (newAmount === 4) {
							expect(amountOfTimesTriggered).to.eq(1);
							expect(extensions?.[0]?.alias).to.eq('Umb.Test.Section.4');
							done();
						}
					}
				})
				.unsubscribe();
		});
	});

	describe('byTypes', () => {
		const types = ['section', 'workspace'];

		it('should get all extensions of the given types', (done) => {
			extensionRegistry
				.byTypes(types)
				.subscribe((extensions) => {
					expect(extensions).to.have.lengthOf(4);
					expect(extensions?.[0]?.type).to.eq('section');
					expect(extensions?.[1]?.type).to.eq('section');
					expect(extensions?.[2]?.type).to.eq('workspace');
					expect(extensions?.[3]?.type).to.eq('section');
					done();
				})
				.unsubscribe();
		});

		it('should return extensions ordered by weight', (done) => {
			extensionRegistry
				.byTypes(types)
				.subscribe((extensions) => {
					expect(extensions?.[0]?.weight).to.eq(200);
					expect(extensions?.[1]?.weight).to.eq(25);
					expect(extensions?.[2]?.weight).to.eq(2);
					expect(extensions?.[3]?.weight).to.eq(1);
					done();
				})
				.unsubscribe();
		});
	});

	describe('getByType', () => {
		it('should get all extensions of the given type', async () => {
			const extensions = extensionRegistry.getByType('section');

			expect(extensions).to.have.lengthOf(3);
			expect(extensions?.[0]?.type).to.eq('section');
			expect(extensions?.[1]?.type).to.eq('section');
			expect(extensions?.[2]?.type).to.eq('section');
		});

		it('should return extensions ordered by weight', () => {
			const extensions = extensionRegistry.getByType('section');

			expect(extensions).to.have.lengthOf(3);
			expect(extensions[0]?.weight).to.eq(200);
			expect(extensions[1]?.weight).to.eq(25);
			expect(extensions[2]?.weight).to.eq(1);
		});
	});
});

describe('UmbExtensionRegistry with kinds', () => {
	let extensionRegistry: UmbExtensionRegistry<any>;
	let manifests: Array<
		ManifestElementWithElementName | TestManifestWithMeta | ManifestKind<ManifestElementWithElementName>
	>;

	beforeEach(() => {
		extensionRegistry = new UmbExtensionRegistry<any>();
		manifests = [
			{
				type: 'kind',
				alias: 'Umb.Test.Kind',
				matchType: 'section',
				matchKind: 'test-kind',
				manifest: {
					type: 'section',
					elementName: 'my-kind-element',
					meta: {
						label: 'my-kind-meta-label',
					},
				},
			},
			{
				type: 'section',
				kind: 'test-kind' as unknown as undefined, // We do not know about this one, so it makes good sense that its not a valid option.
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					//label: 'Test Section 1',// should come from the kind.
					pathname: 'test-section-1',
				},
			},
			{
				type: 'section',
				name: 'test-section-2',
				alias: 'Umb.Test.Section.2',
				weight: 200,
				meta: {
					label: 'Test Section 2',
					pathname: 'test-section-2',
				},
			},
			{
				type: 'section',
				kind: 'test-kind' as unknown as undefined, // We do not know about this one, so it makes good sense that its not a valid option.
				name: 'test-section-3',
				alias: 'Umb.Test.Section.3',
				weight: 25,
				meta: {
					label: 'Test Section 3',
					pathname: 'test-section-3',
				},
			},
			{
				type: 'workspace',
				name: 'test-editor-1',
				alias: 'Umb.Test.Editor.1',
				meta: {
					entityType: 'testEntity',
				},
			},
		];

		manifests.forEach((manifest) => extensionRegistry.register(manifest));
	});
	afterEach(() => {
		extensionRegistry.clear();
	});

	it('should say that an extension kind is registered', () => {
		expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.true;
	});

	it('should merge with kinds', (done) => {
		extensionRegistry
			.byType('section')
			.subscribe((extensions) => {
				expect(extensions).to.have.lengthOf(3);
				expect(extensions?.[0]?.elementName).to.not.eq('my-kind-element');
				expect(extensions?.[1]?.alias).to.eq('Umb.Test.Section.3');
				expect(extensions?.[1]?.meta.label).to.eq('Test Section 3');
				expect(extensions?.[1]?.elementName).to.eq('my-kind-element');
				expect(extensions?.[2]?.alias).to.eq('Umb.Test.Section.1');
				expect(extensions?.[2]?.elementName).to.eq('my-kind-element');
				expect(extensions?.[2]?.meta.label).to.eq('my-kind-meta-label');
				done();
			})
			.unsubscribe();
	});

	it('should update extensions using kinds, when a kind appears', (done) => {
		let amountOfTimesTriggered = -1;

		extensionRegistry.unregister('Umb.Test.Kind');

		extensionRegistry
			.byType('section')
			.subscribe((extensions) => {
				amountOfTimesTriggered++;
				expect(extensions).to.have.lengthOf(3);

				if (amountOfTimesTriggered === 0) {
					expect(extensions?.[2]?.meta.label).to.be.undefined;
					expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.false;
					extensionRegistry.register(manifests[0]); // Registration of the kind again.
					expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.true;
				} else if (amountOfTimesTriggered === 1) {
					expect(extensions?.[2]?.meta.label).to.eq('my-kind-meta-label');
					done();
				}
			})
			.unsubscribe();
	});

	it('should update extensions using kinds, when a kind is removed', (done) => {
		let amountOfTimesTriggered = -1;

		extensionRegistry
			.byType('section')
			.subscribe((extensions) => {
				amountOfTimesTriggered++;
				expect(extensions).to.have.lengthOf(3);

				if (amountOfTimesTriggered === 0) {
					expect(extensions?.[2]?.meta.label).to.not.be.undefined;
					extensionRegistry.unregister('Umb.Test.Kind');
					expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.false;
				} else if (amountOfTimesTriggered === 1) {
					expect(extensions?.[2]?.meta.label).to.be.undefined;
					done();
				}
			})
			.unsubscribe();
	});
});

describe('UmbExtensionRegistry with exclusions', () => {
	let extensionRegistry: UmbExtensionRegistry<any>;
	let manifests: Array<
		ManifestElementWithElementName | TestManifestWithMeta | ManifestKind<ManifestElementWithElementName>
	>;

	beforeEach(() => {
		extensionRegistry = new UmbExtensionRegistry<any>();
		manifests = [
			{
				type: 'kind',
				alias: 'Umb.Test.Kind',
				matchType: 'section',
				matchKind: 'test-kind',
				manifest: {
					type: 'section',
					elementName: 'my-kind-element',
					meta: {
						label: 'my-kind-meta-label',
					},
				},
			},
			{
				type: 'section',
				kind: 'test-kind' as unknown as undefined, // We do not know about this one, so it makes good sense that its not a valid option.
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					pathname: 'test-section-1',
				},
			},
			{
				type: 'section',
				name: 'test-section-2',
				alias: 'Umb.Test.Section.2',
				weight: 200,
				meta: {
					label: 'Test Section 2',
					pathname: 'test-section-2',
				},
			},
		];

		manifests.forEach((manifest) => extensionRegistry.register(manifest));
	});

	it('should have the extensions registered', () => {
		expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.2')).to.be.true;
	});

	it('must not say that Umb.Test.Section.1d is registered, when its added to the exclusion list', () => {
		extensionRegistry.exclude('Umb.Test.Section.1');
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.false;
		// But check that the other ones are still registered:
		expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.2')).to.be.true;
	});

	it('does not affect kinds when a kind-alias is put in the exclusion list', () => {
		extensionRegistry.exclude('Umb.Test.Kind');
		// This had no effect, so all of them are still available.
		expect(extensionRegistry.isRegistered('Umb.Test.Kind')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.2')).to.be.true;
	});

	it('prevents late comers from begin registered', () => {
		extensionRegistry.exclude('Umb.Test.Section.Late');
		extensionRegistry.register({
			type: 'section',
			name: 'test-section-late',
			alias: 'Umb.Test.Section.Late',
			weight: 200,
			meta: {
				label: 'Test Section Late',
				pathname: 'test-section-Late',
			},
		});
		expect(extensionRegistry.isRegistered('Umb.Test.Section.Late')).to.be.false;
	});
});

describe('Add Conditions', () => {
	let extensionRegistry: UmbExtensionRegistry<any>;
	let manifests: Array<ManifestWithDynamicConditions>;

	beforeEach(() => {
		extensionRegistry = new UmbExtensionRegistry<ManifestWithDynamicConditions>();
		manifests = [
			{
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				conditions: [
					{
						alias: 'Umb.Test.Condition.Invalid',
					},
				],
			},
			{
				type: 'section',
				name: 'test-section-2',
				alias: 'Umb.Test.Section.2',
				weight: 200,
			},
		];

		manifests.forEach((manifest) => extensionRegistry.register(manifest));

		extensionRegistry.register({
			type: 'condition',
			name: 'test-condition-invalid',
			alias: 'Umb.Test.Condition.Invalid',
		});
	});

	afterEach(() => {
		extensionRegistry.clear();
	});

	it('should have the extensions registered', () => {
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.2')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Invalid')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Valid')).to.be.false;
	});

	it('allows an extension condition to be updated', async () => {
		const ext = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(1);

		// Register new condition as if I was in my own entrypoint
		extensionRegistry.register({
			type: 'condition',
			name: 'test-condition-valid',
			alias: 'Umb.Test.Condition.Valid',
		});

		// Add the new condition to the extension
		const conditionToAdd: UmbConditionConfigBase = {
			alias: 'Umb.Test.Condition.Valid',
		};
		extensionRegistry.appendCondition('Umb.Test.Section.1', conditionToAdd);

		// Check new condition is registered
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Valid')).to.be.true;

		// Verify the extension now has two conditions and in correct order with aliases
		const updatedExt = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(updatedExt.conditions?.length).to.equal(2);
		expect(updatedExt.conditions?.[0]?.alias).to.equal('Umb.Test.Condition.Invalid');
		expect(updatedExt.conditions?.[1]?.alias).to.equal('Umb.Test.Condition.Valid');

		// Verify the other extension was not updated:
		const otherExt = extensionRegistry.getByAlias('Umb.Test.Section.2') as ManifestWithDynamicConditions;
		expect(otherExt.conditions).to.be.undefined;

		// Add a condition with a specific config to Section2
		const workspaceCondition: WorkspaceAliasConditionConfig = {
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: 'Umb.Workspace.Document',
		};

		extensionRegistry.appendCondition('Umb.Test.Section.2', workspaceCondition);

		const updatedWorkspaceExt = extensionRegistry.getByAlias('Umb.Test.Section.2') as ManifestWithDynamicConditions;
		expect(updatedWorkspaceExt.conditions?.length).to.equal(1);
		expect(updatedWorkspaceExt.conditions?.[0]?.alias).to.equal(UMB_WORKSPACE_CONDITION_ALIAS);
	});

	it('allows an extension to update with multiple conditions', async () => {
		const ext = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(1);

		const conditions: Array<UmbConditionConfigBase> = [
			{
				alias: 'Umb.Test.Condition.Valid',
			},
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			} as WorkspaceAliasConditionConfig,
		];

		extensionRegistry.appendConditions('Umb.Test.Section.1', conditions);

		const extUpdated = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(extUpdated.conditions?.length).to.equal(3);
		expect(extUpdated.conditions?.[0]?.alias).to.equal('Umb.Test.Condition.Invalid');
		expect(extUpdated.conditions?.[1]?.alias).to.equal('Umb.Test.Condition.Valid');
		expect(extUpdated.conditions?.[2]?.alias).to.equal(UMB_WORKSPACE_CONDITION_ALIAS);
	});

	it('allows conditions to be prepended when an extension is loaded later on', async () => {
		const conditions: Array<UmbConditionConfigBase> = [
			{
				alias: 'Umb.Test.Condition.Invalid',
			},
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			} as WorkspaceAliasConditionConfig,
		];

		// Prepend the conditions, but do not await this.
		extensionRegistry.appendConditions('Late.Extension.To.Be.Loaded', conditions);

		// Make sure the extension is not registered YET
		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.false;

		// Register the extension LATE/after the conditions have been added
		extensionRegistry.register({
			type: 'section',
			name: 'Late Section Extension with one condition',
			alias: 'Late.Extension.To.Be.Loaded',
			weight: 200,
			conditions: [
				{
					alias: 'Umb.Test.Condition.Valid',
				},
			],
		});

		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.true;

		const extUpdated = extensionRegistry.getByAlias('Late.Extension.To.Be.Loaded') as ManifestWithDynamicConditions;

		expect(extUpdated.conditions?.length).to.equal(3);
		expect(extUpdated.conditions?.[0]?.alias).to.equal('Umb.Test.Condition.Valid');
		expect(extUpdated.conditions?.[1]?.alias).to.equal('Umb.Test.Condition.Invalid');
		expect(extUpdated.conditions?.[2]?.alias).to.equal(UMB_WORKSPACE_CONDITION_ALIAS);
	});

	/**
	 * As of current state, it is by design without further reasons to why, but it is made so additional conditions are only added to a current or next time registered manifest.
	 * Meaning if it happens to be unregistered and re-registered it does not happen again.
	 * Unless the exact same appending of conditions happens again. [NL]
	 *
	 * This makes sense if extensions gets offloaded and re-registered, but the extension that registered additional conditions didn't get loaded/registered second time. Therefor they need to be re-registered for such to work. [NL]
	 */
	it('only append conditions to the next time the extension is registered', async () => {
		const conditions: Array<UmbConditionConfigBase> = [
			{
				alias: 'Umb.Test.Condition.Invalid',
			},
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			} as WorkspaceAliasConditionConfig,
		];

		// Prepend the conditions, but do not await this.
		extensionRegistry.appendConditions('Late.Extension.To.Be.Loaded', conditions);

		// Make sure the extension is not registered YET
		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.false;

		// Register the extension LATE/after the conditions have been added
		extensionRegistry.register({
			type: 'section',
			name: 'Late Section Extension with one condition',
			alias: 'Late.Extension.To.Be.Loaded',
			weight: 200,
			conditions: [
				{
					alias: 'Umb.Test.Condition.Valid',
				},
			],
		});

		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.true;

		const extUpdateFirstTime = extensionRegistry.getByAlias(
			'Late.Extension.To.Be.Loaded',
		) as ManifestWithDynamicConditions;
		expect(extUpdateFirstTime.conditions?.length).to.equal(3);

		extensionRegistry.unregister('Late.Extension.To.Be.Loaded');

		// Make sure the extension is not registered YET
		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.false;

		// Register the extension LATE/after the conditions have been added
		extensionRegistry.register({
			type: 'section',
			name: 'Late Section Extension with one condition',
			alias: 'Late.Extension.To.Be.Loaded',
			weight: 200,
			conditions: [
				{
					alias: 'Umb.Test.Condition.Valid',
				},
			],
		});

		expect(extensionRegistry.isRegistered('Late.Extension.To.Be.Loaded')).to.be.true;

		const extUpdateSecondTime = extensionRegistry.getByAlias(
			'Late.Extension.To.Be.Loaded',
		) as ManifestWithDynamicConditions;

		expect(extUpdateSecondTime.conditions?.length).to.equal(1);
		expect(extUpdateSecondTime.conditions?.[0]?.alias).to.equal('Umb.Test.Condition.Valid');
	});

	it('should only update extensions when adding new conditions to one that matters', (done) => {
		let amountOfTimesTriggered = -1;

		let subscription: any = undefined;
		subscription = extensionRegistry
			.byTypeAndAliases('section', ['Umb.Test.Section.1'])
			.subscribe(async (extensions) => {
				amountOfTimesTriggered++;
				expect(extensions).to.have.lengthOf(1);

				if (amountOfTimesTriggered === 0) {
					expect(extensions?.[0]?.alias).to.be.equal('Umb.Test.Section.1');
					expect(extensions?.[0]?.conditions.length).to.be.equal(1);
					// Add a condition with a specific config to Section2

					const workspaceCondition1 = {
						alias: 'Umb.Test.Condition.Invalid',
					};

					const workspaceCondition2 = {
						alias: 'Umb.Test.Condition.Valid',
					};

					await Promise.resolve();

					extensionRegistry.appendCondition('Umb.Test.Section.2', workspaceCondition1);

					await Promise.resolve();

					extensionRegistry.appendCondition('Umb.Test.Section.1', workspaceCondition2);
				} else if (amountOfTimesTriggered === 1) {
					expect(extensions?.[0]?.alias).to.be.equal('Umb.Test.Section.1');
					expect(extensions?.[0]?.conditions.length).to.be.equal(2);
					expect(extensions?.[0]?.conditions[1].alias).to.be.equal('Umb.Test.Condition.Valid');

					subscription.unsubscribe();
					done();
				}
			});
	});
	it('should update extensions when adding new conditions', (done) => {
		let amountOfTimesTriggered = -1;

		extensionRegistry
			.byType('section')
			.subscribe(async (extensions) => {
				amountOfTimesTriggered++;
				expect(extensions).to.have.lengthOf(2);

				if (amountOfTimesTriggered === 0) {
					expect(extensions?.[1]?.alias).to.be.equal('Umb.Test.Section.1');
					expect(extensions?.[1]?.conditions.length).to.be.equal(1);
					expect(extensions?.[0]?.alias).to.be.equal('Umb.Test.Section.2');
					expect(extensions?.[0]?.conditions).to.be.undefined;
					// Add a condition with a specific config to Section2
					const workspaceCondition: WorkspaceAliasConditionConfig = {
						alias: UMB_WORKSPACE_CONDITION_ALIAS,
						match: 'Umb.Workspace.Document',
					};

					extensionRegistry.appendCondition('Umb.Test.Section.2', workspaceCondition);
				} else if (amountOfTimesTriggered === 1) {
					expect(extensions?.[0]?.alias).to.be.equal('Umb.Test.Section.2');
					expect(extensions?.[0]?.conditions).to.not.be.undefined;
					expect(extensions?.[0]?.conditions.length).to.be.equal(1);
					done();
				}
			})
			.unsubscribe();
	});
});
