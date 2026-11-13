import { UmbContentPickerToMediaClipboardCopyPropertyValueTranslator } from './content-picker-to-media-copy-translator.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';

@customElement('test-content-picker-to-media-copy-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbContentPickerToMediaClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbContentPickerToMediaClipboardCopyPropertyValueTranslator;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbContentPickerToMediaClipboardCopyPropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps only media references to bare media references', async () => {
		const result = await copyTranslator.translate([
			{ type: UMB_MEDIA_ENTITY_TYPE, unique: 'media-1' },
			{ type: 'document', unique: 'doc-1' },
			{ type: UMB_MEDIA_ENTITY_TYPE, unique: 'media-2' },
		]);
		expect(result).to.deep.equal([{ unique: 'media-1' }, { unique: 'media-2' }]);
	});

	it('returns an empty array when there are no media references', async () => {
		const result = await copyTranslator.translate([{ type: 'document', unique: 'doc-1' }]);
		expect(result).to.deep.equal([]);
	});

	it('throws when the property value is missing', async () => {
		let error: unknown;
		try {
			await copyTranslator.translate(undefined as never);
		} catch (e) {
			error = e;
		}
		expect(error).to.be.instanceOf(Error);
	});
});
