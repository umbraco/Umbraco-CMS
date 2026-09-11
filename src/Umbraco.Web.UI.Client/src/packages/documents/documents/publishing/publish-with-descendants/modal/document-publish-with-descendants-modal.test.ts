import { UmbDocumentVariantState } from '../../../variant-state.js';
import type { UmbDocumentPublishWithDescendantsModalElement } from './document-publish-with-descendants-modal.element.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { expect, fixture, html } from '@open-wc/testing';

import './document-publish-with-descendants-modal.element.js';

function option(
	unique: string,
	variant: Partial<UmbDocumentVariantOptionModel['variant']> | undefined,
	language: Partial<UmbDocumentVariantOptionModel['language']> = {},
): UmbDocumentVariantOptionModel {
	return {
		unique,
		culture: unique,
		segment: null,
		variant: variant
			? ({
					name: unique,
					culture: unique,
					segment: null,
					state: UmbDocumentVariantState.DRAFT,
					createDate: null,
					publishDate: null,
					updateDate: null,
					scheduledPublishDate: null,
					scheduledUnpublishDate: null,
					flags: [],
					...variant,
				} as UmbDocumentVariantOptionModel['variant'])
			: undefined,
		language: {
			entityType: 'language',
			name: unique,
			unique,
			isDefault: false,
			isMandatory: false,
			fallbackIsoCode: null,
			...language,
		},
	} as UmbDocumentVariantOptionModel;
}

async function listedOptions(options: Array<UmbDocumentVariantOptionModel>): Promise<Array<string>> {
	const element = await fixture<UmbDocumentPublishWithDescendantsModalElement>(
		html`<umb-document-publish-with-descendants-modal
			.data=${{ options }}></umb-document-publish-with-descendants-modal>`,
	);
	await element.updateComplete;

	const picker = element.shadowRoot!.querySelector('umb-document-variant-language-picker');
	expect(picker, 'the variant picker is rendered').to.exist;

	return picker!.variantLanguageOptions.map((o) => o.unique);
}

describe('UmbDocumentPublishWithDescendantsModalElement', () => {
	// A variant the document does not hold any data for cannot be published, so listing it offers the user
	// a choice that can never be acted on. (#23886)
	it('does not list variants that have not been created', async () => {
		const listed = await listedOptions([
			option('en-us', { state: UmbDocumentVariantState.PUBLISHED }),
			option('da-dk', { state: UmbDocumentVariantState.NOT_CREATED }),
			option('de-de', undefined),
		]);

		expect(listed).to.eql(['en-us']);
	});

	// A mandatory language has to be publishable from here even before it holds data, as publishing cannot
	// complete without it.
	it('lists a mandatory variant that has not been created', async () => {
		const listed = await listedOptions([
			option('en-us', { state: UmbDocumentVariantState.PUBLISHED }),
			option('da-dk', undefined, { isMandatory: true }),
		]);

		expect(listed).to.have.members(['en-us', 'da-dk']);
	});

	// A variant whose state is unknown may well hold data, so it stays available for selection.
	it('lists variants of unknown state', async () => {
		const listed = await listedOptions([option('en-us', { state: null })]);

		expect(listed).to.eql(['en-us']);
	});
});
