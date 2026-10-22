import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbFlatLayoutBlockPropertyValueCloner } from './flat-layout-block-property-value-cloner.api';
import { UmbPropertyValueCloneController, type ManifestPropertyValueCloner } from '@umbraco-cms/backoffice/property';
import type { UmbBlockValueType } from '../types';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class TestUmbFlatLayoutBlockPropertyValueCloner extends UmbFlatLayoutBlockPropertyValueCloner {
	constructor() {
		super('TestEditor');
	}
}

describe('FlatLayoutBlockPropertyValueCloner', () => {
	describe('Cloner', () => {
		beforeEach(async () => {
			const manifestCloner: ManifestPropertyValueCloner = {
				type: 'propertyValueCloner',
				name: 'test-cloner-1',
				alias: 'Umb.Test.Cloner.1',
				api: TestUmbFlatLayoutBlockPropertyValueCloner,
				forEditorAlias: 'block-test-editor',
			};

			umbExtensionsRegistry.register(manifestCloner);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Cloner.1');
		});

		it('clones value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'block-test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					layout: {
						TestEditor: [
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
			};

			const result = (await ctrl.clone(value)) as { value: UmbBlockValueType | undefined };

			const newContentKey = result.value?.layout.TestEditor?.[0].contentKey;
			const newSettingsKey = result.value?.layout.TestEditor?.[0].settingsKey;

			expect(result.value?.layout.TestEditor?.[0].contentKey).to.not.be.equal('content-1');
			expect(result.value?.layout.TestEditor?.[0].settingsKey).to.not.be.equal('settings-1');
			expect(result.value?.layout.TestEditor?.[0].contentKey).to.be.equal(newContentKey);

			expect(result.value?.contentData[0].key).to.not.be.equal('content-1');
			expect(result.value?.contentData[0].key).to.be.equal(newContentKey);
			expect(result.value?.settingsData[0].key).to.not.be.equal('settings-1');
			expect(result.value?.settingsData[0].key).to.be.equal(newSettingsKey);
		});

		it('keeps the contentKey of external content while generating a new layout key', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'block-test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					layout: {
						TestEditor: [
							{
								key: 'layout-1',
								contentKey: 'external-content-1',
								settingsKey: null,
								isExternalContent: true,
							},
						],
					},
					contentData: [],
					settingsData: [],
					expose: [],
				},
			};

			const result = (await ctrl.clone(value)) as { value: UmbBlockValueType | undefined };
			const layoutEntry = result.value?.layout.TestEditor?.[0];

			expect(layoutEntry?.contentKey).to.be.equal('external-content-1');
			expect(layoutEntry?.key).to.not.be.equal('layout-1');
			expect(layoutEntry?.isExternalContent).to.be.true;
		});

		it('only renews the contentKey of local blocks when mixed with external content', async () => {
			const updatedKeys: Array<string> = [];
			const cloner = new UmbFlatLayoutBlockPropertyValueCloner('TestEditor', {
				contentIdUpdatedCallback: (oldContentKey) => updatedKeys.push(oldContentKey),
			});

			const value: UmbBlockValueType = {
				layout: {
					TestEditor: [
						{ key: 'layout-1', contentKey: 'content-1', settingsKey: null },
						{ key: 'layout-2', contentKey: 'external-content-1', settingsKey: null, isExternalContent: true },
					],
				},
				contentData: [{ key: 'content-1', contentTypeKey: 'fictive-content-type', values: [] }],
				settingsData: [],
				expose: [{ contentKey: 'content-1', culture: null }],
			};

			const result = await cloner.cloneValue(value);
			const [local, external] = result.layout.TestEditor!;

			expect(local.contentKey).to.not.be.equal('content-1');
			expect(result.contentData[0].key).to.be.equal(local.contentKey);
			expect(result.expose[0].contentKey).to.be.equal(local.contentKey);

			expect(external.contentKey).to.be.equal('external-content-1');
			expect(external.key).to.not.be.equal('layout-2');

			expect(updatedKeys).to.deep.equal(['content-1']);
		});
	});
});
