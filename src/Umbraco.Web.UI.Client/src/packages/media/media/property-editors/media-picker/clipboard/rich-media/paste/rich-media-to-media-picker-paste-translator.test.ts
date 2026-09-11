import { UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator } from './rich-media-to-media-picker-paste-translator.js';
import type { UmbRichMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

@customElement('test-rich-media-to-media-picker-paste-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const SQUARE = { alias: 'square', width: 100, height: 100 };
const COORDINATES = { x1: 0.1, x2: 0.2, y1: 0.3, y2: 0.4 };

function config(options?: { crops?: Array<unknown>; enableLocalFocalPoint?: boolean }): UmbPropertyEditorConfig {
	return [
		{ alias: 'crops', value: options?.crops ?? [] },
		{ alias: 'enableLocalFocalPoint', value: options?.enableLocalFocalPoint ?? false },
	];
}

describe('UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator;

	const clipboardValue: UmbRichMediaClipboardEntryValueModel = [
		{ unique: 'media-1', focalPoint: { left: 0.4, top: 0.6 }, crops: [{ ...SQUARE, coordinates: COORDINATES }] },
	];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each rich media reference to a media picker entry', async () => {
		const result = await pasteTranslator.translate(clipboardValue, config({ crops: [SQUARE] }));
		expect(result).to.have.lengthOf(1);
		const [entry] = result;
		expect(entry.key).to.be.a('string');
		expect(entry.key.length).to.be.greaterThan(0);
		expect(entry.mediaKey).to.equal('media-1');
		expect(entry.mediaTypeAlias).to.equal('');
	});

	it('generates a new unique key per entry', async () => {
		const result = await pasteTranslator.translate(
			[
				{ unique: 'a', focalPoint: null, crops: [] },
				{ unique: 'b', focalPoint: null, crops: [] },
			],
			config(),
		);
		expect(result[0].key).to.not.equal(result[1].key);
	});

	it('throws when the value is missing', async () => {
		let error: unknown;
		try {
			await pasteTranslator.translate(undefined as never, config());
		} catch (e) {
			error = e;
		}
		expect(error).to.be.instanceOf(Error);
	});

	it('accepts any rich media value (isCompatibleValue)', async () => {
		expect(await pasteTranslator.isCompatibleValue()).to.be.true;
	});

	describe('crops', () => {
		it('keeps a framed crop the configuration supports, at the configured size', async () => {
			const value: UmbRichMediaClipboardEntryValueModel = [
				{ unique: 'media-1', focalPoint: null, crops: [{ alias: 'square', width: 200, height: 200, coordinates: COORDINATES }] },
			];

			const [entry] = await pasteTranslator.translate(value, config({ crops: [SQUARE] }));

			// The configuration defines the crop; the pasted value only contributes the framing.
			expect(entry.crops).to.deep.equal([{ ...SQUARE, coordinates: COORDINATES }]);
		});

		it('drops a framed crop the configuration does not declare', async () => {
			const value: UmbRichMediaClipboardEntryValueModel = [
				{
					unique: 'media-1',
					focalPoint: null,
					crops: [
						{ ...SQUARE, coordinates: COORDINATES },
						{ alias: 'banner', width: 600, height: 200, coordinates: COORDINATES },
					],
				},
			];

			const [entry] = await pasteTranslator.translate(value, config({ crops: [SQUARE] }));

			// A crop the property editor cannot edit must not arrive through a paste.
			expect(entry.crops.map((crop) => crop.alias)).to.deep.equal(['square']);
		});

		it('drops a framed crop whose aspect ratio does not match the configured one', async () => {
			const value: UmbRichMediaClipboardEntryValueModel = [
				{ unique: 'media-1', focalPoint: null, crops: [{ alias: 'square', width: 200, height: 100, coordinates: COORDINATES }] },
			];

			const [entry] = await pasteTranslator.translate(value, config({ crops: [SQUARE] }));

			// The framing was set against a different shape, so it would no longer mean what the user chose.
			expect(entry.crops).to.deep.equal([]);
		});

		it('drops a crop with no framing, which the server adds anyway', async () => {
			const value: UmbRichMediaClipboardEntryValueModel = [{ unique: 'media-1', focalPoint: null, crops: [SQUARE] }];

			const [entry] = await pasteTranslator.translate(value, config({ crops: [SQUARE] }));

			// Same as a freshly picked media, which arrives with no crops at all.
			expect(entry.crops).to.deep.equal([]);
		});

		it('drops every crop when the configuration declares none', async () => {
			const [entry] = await pasteTranslator.translate(clipboardValue, config());
			expect(entry.crops).to.deep.equal([]);
		});
	});

	describe('focal point', () => {
		it('keeps the focal point when local focal point is enabled', async () => {
			const [entry] = await pasteTranslator.translate(clipboardValue, config({ enableLocalFocalPoint: true }));
			expect(entry.focalPoint).to.deep.equal({ left: 0.4, top: 0.6 });
		});

		it('drops the focal point when local focal point is not enabled', async () => {
			const [entry] = await pasteTranslator.translate(clipboardValue, config({ enableLocalFocalPoint: false }));
			expect(entry.focalPoint).to.be.null;
		});
	});
});
