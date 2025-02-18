import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { manifests } from './manifests';
import {
	UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	type UmbPropertyEditorUiValueType,
} from '@umbraco-cms/backoffice/rte';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockRtePropertyValueCloner', () => {
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
				editorAlias: UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					markup:
						'<p>the upper markup</p><umb-rte-block-inline data-content-key="content-1"><!--a comment?--></umb-rte-block-inline><b>the middle markup</b><umb-rte-block data-content-key="content-2"><!--a comment?--></umb-rte-block><p>the lower markup</p>',
					blocks: {
						layout: {
							[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: [
								{
									contentKey: 'content-1',
									settingsKey: 'settings-1',
								},
							],
						},
						contentData: [
							{
								key: 'content-1',
								contentTypeKey: 'fictive-content-type',
								culture: null,
								segment: null,
							},
						],
						settingsData: [
							{
								key: 'settings-1',
								contentTypeKey: 'fictive-content-type',
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
						],
					},
				},
			};

			const result = (await ctrl.clone(value)) as { value: UmbPropertyEditorUiValueType | undefined };

			const newContentKey = result.value?.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey;
			const newSettingsKey = result.value?.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].settingsKey;

			if (newContentKey === undefined) {
				throw new Error('newContentKey is undefined');
			}

			expect(result.value?.markup.indexOf(newContentKey) !== 0).to.be.true;

			expect(result.value?.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey).to.not.be.equal(
				'content-1',
			);
			expect(result.value?.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].settingsKey).to.not.be.equal(
				'settings-1',
			);
			expect(result.value?.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.[0].contentKey).to.be.equal(
				newContentKey,
			);

			expect(result.value?.blocks.contentData[0].key).to.not.be.equal('content-1');
			expect(result.value?.blocks.contentData[0].key).to.be.equal(newContentKey);
			expect(result.value?.blocks.settingsData[0].key).to.not.be.equal('settings-1');
			expect(result.value?.blocks.settingsData[0].key).to.be.equal(newSettingsKey);
		});
	});
});
