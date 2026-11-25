import { UmbContentPickerToDocumentPickerConfigConverter } from './content-picker-to-document-picker-config-converter.js';
import { UmbContentPickerToMediaPickerConfigConverter } from './content-picker-to-media-picker-config-converter.js';
import { UmbContentPickerToMemberPickerConfigConverter } from './content-picker-to-member-picker-config-converter.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-content-picker-config-converter-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const dynamicRoot = {
	originAlias: 'Root',
	querySteps: [{ unique: 'step-1', alias: 'NearestDescendantOrSelf', anyOfDocTypeKeys: ['section-type'] }],
};

describe('Multi Node Tree Picker config converters', () => {
	let host: UmbTestControllerHostElement;

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		document.body.innerHTML = '';
		document.body.appendChild(host);
	});

	describe('UmbContentPickerToDocumentPickerConfigConverter', () => {
		it('renames the filter, and moves the start node and dynamic root out of the start node', () => {
			const result = new UmbContentPickerToDocumentPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'content', id: 'start-node', dynamicRoot } },
				{ alias: 'filter', value: 'type-1,type-2' },
				{ alias: 'ignoreUserStartNodes', value: true },
			]);

			expect(result).to.deep.equal([
				{ alias: 'allowedContentTypes', value: 'type-1,type-2' },
				{ alias: 'startNodeId', value: 'start-node' },
				{ alias: 'dynamicRoot', value: dynamicRoot },
			]);
		});

		it('treats a missing start node type as documents', () => {
			const result = new UmbContentPickerToDocumentPickerConfigConverter(host).convert([
				{ alias: 'filter', value: 'type-1' },
			]);

			expect(result).to.deep.equal([{ alias: 'allowedContentTypes', value: 'type-1' }]);
		});

		it('leaves out null and empty settings', () => {
			const result = new UmbContentPickerToDocumentPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'content', id: null, dynamicRoot: null, query: null } },
				{ alias: 'filter', value: '' },
			]);

			expect(result).to.have.lengthOf(0);
		});

		it('converts nothing for a media or member start node', () => {
			const result = new UmbContentPickerToDocumentPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'media', id: 'media-folder' } },
				{ alias: 'filter', value: 'image' },
			]);

			expect(result).to.have.lengthOf(0);
		});
	});

	describe('UmbContentPickerToMediaPickerConfigConverter', () => {
		it('moves the media start node out of the start node', () => {
			const result = new UmbContentPickerToMediaPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'media', id: 'media-folder', dynamicRoot } },
				{ alias: 'filter', value: 'image' },
			]);

			expect(result).to.deep.equal([{ alias: 'startNodeId', value: 'media-folder' }]);
		});

		it('clears the filter when the start node is for documents', () => {
			const result = new UmbContentPickerToMediaPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'content', id: 'start-node' } },
				{ alias: 'filter', value: 'document-type' },
			]);

			expect(result).to.deep.equal([{ alias: 'filter', value: null }]);
		});
	});

	describe('UmbContentPickerToMemberPickerConfigConverter', () => {
		it('converts nothing for a member start node, as the filter carries over', () => {
			const result = new UmbContentPickerToMemberPickerConfigConverter(host).convert([
				{ alias: 'startNode', value: { type: 'member' } },
				{ alias: 'filter', value: 'member-type' },
			]);

			expect(result).to.have.lengthOf(0);
		});

		it('clears the filter when the start node is for documents', () => {
			const result = new UmbContentPickerToMemberPickerConfigConverter(host).convert([
				{ alias: 'filter', value: 'document-type' },
			]);

			expect(result).to.deep.equal([{ alias: 'filter', value: null }]);
		});
	});
});
