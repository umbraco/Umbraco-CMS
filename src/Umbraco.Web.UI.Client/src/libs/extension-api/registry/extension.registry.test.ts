import { WorkspaceAliasConditionConfig } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestElementWithElementName,
	ManifestKind,
	ManifestBase,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import { UmbExtensionRegistry } from './extension.registry.js';
import { expect } from '@open-wc/testing';

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

describe('Append Conditions', () => {
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
						alias: 'Umb.Test.Condition.Valid',
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
		await extensionRegistry.appendCondition('Umb.Test.Section.1', conditionToAdd);

		// Check new condition is registered
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Valid')).to.be.true;

		// Verify the extension now has two conditions
		const updatedExt = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(updatedExt.conditions?.length).to.equal(2);

		// Add a condition with a specific config to Section2
		const workspaceCondition: WorkspaceAliasConditionConfig = {
			alias: 'Umb.Condition.WorkspaceAlias',
			match: 'Umb.Workspace.Document',
		};
		await extensionRegistry.appendCondition('Umb.Test.Section.2', workspaceCondition);

		const updatedWorkspaceExt = extensionRegistry.getByAlias('Umb.Test.Section.2') as ManifestWithDynamicConditions;
		expect(updatedWorkspaceExt.conditions?.length).to.equal(1);
	});

	it('allows an extension to update with multiple conditions', async () => {
		const ext = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(1);

		const conditions: Array<UmbConditionConfigBase> = [
			{
				alias: 'Umb.Test.Condition.Valid',
			},
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			} as WorkspaceAliasConditionConfig,
		];

		await extensionRegistry.appendConditions('Umb.Test.Section.1', conditions);

		const extUpdated = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(3);
	});
});

describe('Prepend Conditions', () => {
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
						alias: 'Umb.Test.Condition.Valid',
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

	it('should have the extensions registered', () => {
		expect(extensionRegistry.isRegistered('Umb.Test.Section.1')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Section.2')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Invalid')).to.be.true;
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Valid')).to.be.false;
	});

	it('allows an extension condition to be prepended', async () => {
		const ext = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(1);

		// Register new condition as if I was in my own entrypoint
		extensionRegistry.register({
			type: 'condition',
			name: 'test-condition-valid',
			alias: 'Umb.Test.Condition.Valid',
		});

		// Prepend the new condition to the extension
		const conditionToPrepend: UmbConditionConfigBase = {
			alias: 'Umb.Test.Condition.Valid',
		};

		await extensionRegistry.prependCondition('Umb.Test.Section.1', conditionToPrepend);

		// Check new condition is registered
		expect(extensionRegistry.isRegistered('Umb.Test.Condition.Valid')).to.be.true;

		// Verify the extension now has two conditions and the new condition is prepended
		const updatedExt = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(updatedExt.conditions?.length).to.equal(2);
		expect(updatedExt.conditions?.[0]?.alias).to.equal('Umb.Test.Condition.Valid');
	});

	it('allows an extension to update with multiple prepended conditions', async () => {
		const ext = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(ext.conditions?.length).to.equal(1);

		const conditions: Array<UmbConditionConfigBase> = [
			{
				alias: 'Umb.Test.Condition.Valid',
			},
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			} as WorkspaceAliasConditionConfig,
		];

		await extensionRegistry.prependConditions('Umb.Test.Section.1', conditions);

		const extUpdated = extensionRegistry.getByAlias('Umb.Test.Section.1') as ManifestWithDynamicConditions;
		expect(extUpdated.conditions?.length).to.equal(3);
		expect(extUpdated.conditions?.[0]?.alias).to.equal('Umb.Condition.WorkspaceAlias');
		expect(extUpdated.conditions?.[1]?.alias).to.equal('Umb.Test.Condition.Valid');
	});
});
