import { expect } from '@open-wc/testing';
import type { ManifestTypes } from '../../models';
import { UmbExtensionRegistry } from './extension.registry';

describe('UmbContextRequestEvent', () => {
	let extensionRegistry: UmbExtensionRegistry;
	let manifests: Array<ManifestTypes>;

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
		expect(registeredExtensions?.[0]?.name).to.eq('test-section-1');
	});

	it('should get an extension by alias', (done) => {
		const alias = 'Umb.Test.Section.1';
		extensionRegistry.getByAlias(alias).subscribe((extension) => {
			expect(extension?.alias).to.eq(alias);
			done();
		});
	});

	describe('getByType', () => {
		const type = 'section';

		it('should get all extensions by type', (done) => {
			extensionRegistry.extensionsOfType(type).subscribe((extensions) => {
				expect(extensions).to.have.lengthOf(3);
				expect(extensions?.[0]?.type).to.eq(type);
				expect(extensions?.[1]?.type).to.eq(type);
				done();
			});
		});

		it('should return extensions ordered by weight', (done) => {
			extensionRegistry.extensionsOfType(type).subscribe((extensions) => {
				expect(extensions?.[0]?.weight).to.eq(200);
				expect(extensions?.[1]?.weight).to.eq(25);
				expect(extensions?.[2]?.weight).to.eq(1);
				done();
			});
		});
	});
});
