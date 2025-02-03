import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { manifests } from './manifests';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants';
import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../types';
import type { UmbBlockDataModel, UmbBlockExposeModel } from '@umbraco-cms/backoffice/block';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockGridPropertyValueCloner', () => {
	describe('Cloner', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.registerMany(manifests);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregisterMany(manifests.map((m) => m.alias));
		});

		it('clones value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					layout: {
						[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: [
							{
								contentKey: 'content-1',
								settingsKey: 'settings-1',
								areas: [
									{
										alias: 'area-1',
										items: [
											{
												contentKey: 'content-2',
												settingsKey: 'settings-2',
												areas: [
													{
														alias: 'area-2',
														items: [
															{
																contentKey: 'content-3',
																settingsKey: 'settings-3',
															},
														],
													},
												],
											},
										],
									},
								],
							},
						],
					},
					contentData: [
						{
							key: 'content-1',
							contentTypeKey: 'fictive-content-type-1',
							culture: null,
							segment: null,
						},
						{
							key: 'content-2',
							contentTypeKey: 'fictive-content-type-2',
							culture: null,
							segment: null,
						},
						{
							key: 'content-3',
							contentTypeKey: 'fictive-content-type-3',
							culture: null,
							segment: null,
						},
					],
					settingsData: [
						{
							key: 'settings-1',
							contentTypeKey: 'fictive-content-type-1',
							culture: null,
							segment: null,
						},
						{
							key: 'settings-2',
							contentTypeKey: 'fictive-content-type-2',
							culture: null,
							segment: null,
						},
						{
							key: 'settings-3',
							contentTypeKey: 'fictive-content-type-3',
							culture: null,
							segment: null,
						},
					],
					expose: [
						{
							contentKey: 'content-1',
							culture: null,
							segment: null,
						},
						{
							contentKey: 'content-2',
							culture: null,
							segment: null,
						},
						{
							contentKey: 'content-3',
							culture: null,
							segment: null,
						},
					],
				},
			};

			const result = (await ctrl.clone(value)) as { value: UmbBlockGridValueModel | undefined };

			const newContentKey = result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey;
			const newSettingsKey = result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].settingsKey;

			if (newContentKey === undefined) {
				throw new Error('newContentKey is undefined');
			}

			expect(result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey).to.not.be.equal(
				'content-1',
			);
			expect(result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].settingsKey).to.not.be.equal(
				'settings-1',
			);
			expect(result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey).to.be.equal(
				newContentKey,
			);

			expect(result.value?.contentData[0].key).to.not.be.equal('content-1');
			expect(result.value?.contentData[0].key).to.be.equal(newContentKey);
			expect(result.value?.settingsData[0].key).to.not.be.equal('settings-1');
			expect(result.value?.settingsData[0].key).to.be.equal(newSettingsKey);
			expect(result.value?.expose[0].contentKey).to.not.be.equal('content-1');
			expect(result.value?.expose[0].contentKey).to.be.equal(newContentKey);

			testLayoutEntryNewKeyIsReflected(
				'fictive-content-type-1',
				result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0],
				result.value?.contentData,
				result.value?.settingsData,
				result.value?.expose,
			);

			// Test for inner layout entry:

			expect(result.value?.contentData[1].key).to.not.be.equal('content-2');
			expect(result.value?.settingsData[1].key).to.not.be.equal('settings-2');

			testLayoutEntryNewKeyIsReflected(
				'fictive-content-type-2',
				result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].areas?.[0]?.items[0],
				result.value?.contentData,
				result.value?.settingsData,
				result.value?.expose,
			);

			// Test for inner inner layout entry:

			expect(result.value?.contentData[2].key).to.not.be.equal('content-3');
			expect(result.value?.settingsData[2].key).to.not.be.equal('settings-3');

			testLayoutEntryNewKeyIsReflected(
				'fictive-content-type-3',
				result.value?.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].areas?.[0]?.items[0].areas?.[0]
					?.items[0],
				result.value?.contentData,
				result.value?.settingsData,
				result.value?.expose,
			);
		});
	});
});

function testLayoutEntryNewKeyIsReflected(
	contentTypeKey: string,
	layoutEntry?: UmbBlockGridLayoutModel,
	contentData?: Array<UmbBlockDataModel>,
	settingsData?: Array<UmbBlockDataModel>,
	expose?: Array<UmbBlockExposeModel>,
) {
	if (!layoutEntry || !contentData || !settingsData || !expose) {
		throw new Error('some arguments was undefined');
	}
	// Test layout entry
	const newContentKey = layoutEntry.contentKey;
	const newSettingsKey = layoutEntry.settingsKey;

	expect(contentData.some((x) => x.key === newContentKey && x.contentTypeKey === contentTypeKey)).to.be.true;
	expect(settingsData.some((x) => x.key === newSettingsKey && x.contentTypeKey === contentTypeKey)).to.be.true;
	expect(expose.some((x) => x.contentKey === newContentKey)).to.be.true;
}
