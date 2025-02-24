import { UmbDocumentVariantState } from '../../../types.js';
import type {
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue,
} from './document-publish-with-descendants-modal.token.js';
import type { UmbDocumentPublishWithDescendantsModalElement } from './document-publish-with-descendants-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './document-publish-with-descendants-modal.element.js';

const modalData: UmbDocumentPublishWithDescendantsModalData = {
	options: [
		{
			unique: 'en-us',
			culture: 'en-us',
			segment: null,
			variant: {
				name: 'English variant name',
				culture: 'en-us',
				state: UmbDocumentVariantState.PUBLISHED,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
				segment: null,
				scheduledPublishDate: null,
				scheduledUnpublishDate: null,
			},
			language: {
				entityType: 'language',
				name: 'English',
				unique: 'en-us',
				isDefault: true,
				isMandatory: true,
				fallbackIsoCode: null,
			},
		},
		{
			unique: 'en-gb',
			culture: 'en-gb',
			segment: null,
			variant: {
				name: 'English (GB)',
				culture: 'en-us',
				segment: null,
				state: UmbDocumentVariantState.DRAFT,
				createDate: '2021-08-25T14:00:00Z',
				publishDate: null,
				updateDate: null,
				scheduledPublishDate: null,
				scheduledUnpublishDate: null,
			},
			language: {
				entityType: 'language',
				name: 'English (GB)',
				unique: 'en-gb',
				isDefault: true,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		},
		{
			unique: 'da-dk',
			culture: 'da-dk',
			segment: null,
			variant: {
				name: 'Danish variant name',
				culture: 'da-dk',
				state: UmbDocumentVariantState.NOT_CREATED,
				createDate: null,
				publishDate: null,
				updateDate: null,
				segment: null,
				scheduledPublishDate: null,
				scheduledUnpublishDate: null,
			},
			language: {
				entityType: 'language',
				name: 'Danish',
				unique: 'da-dk',
				isDefault: false,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		},
	],
};

const modalValue: UmbDocumentPublishWithDescendantsModalValue = {
	selection: ['en-us'],
};

const meta: Meta<UmbDocumentPublishWithDescendantsModalElement> = {
	title: 'Workspaces/Document/Modals/Publish With Descendants Modal',
	component: 'umb-document-publish-with-descendants-modal',
	id: 'umb-document-publish-with-descendants-modal',
	args: {
		data: modalData,
		value: modalValue,
	},
	decorators: [(Story) => html`<div style="border: 1px solid #000;">${Story()}</div>`],
	parameters: {
		layout: 'centered',
		docs: {
			source: {
				language: 'ts',
				code: `
import { UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, UmbDocumentVariantState } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
	const result = modalManager.open(this, UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, {
		data: {
			options: [
				{
					unique: 'en-us',
					culture: 'en-us',
					segment: null,
					variant: {
						name: 'English variant name',
						culture: 'en-us',
						state: UmbDocumentVariantState.PUBLISHED,
						createDate: '2021-08-25T14:00:00Z',
						publishDate: null,
						updateDate: null,
						segment: null,
					},
					language: {
						entityType: 'language',
						name: 'English',
						unique: 'en-us',
						isDefault: true,
						isMandatory: true,
						fallbackIsoCode: null,
					},
				},
				{
					unique: 'da-dk',
					culture: 'da-dk',
					segment: null,
					variant: {
						name: 'Danish variant name',
						culture: 'da-dk',
						state: UmbDocumentVariantState.NOT_CREATED,
						createDate: null,
						publishDate: null,
						updateDate: null,
						segment: null,
					},
					language: {
						entityType: 'language',
						name: 'Danish',
						unique: 'da-dk',
						isDefault: false,
						isMandatory: false,
						fallbackIsoCode: null,
					},
				},
			],
		}
	}).onSubmit().catch(() => undefined);
});
				`,
			},
		},
	},
};

export default meta;
type Story = StoryObj<UmbDocumentPublishWithDescendantsModalElement>;

export const Overview: Story = {};

export const Invariant: Story = {
	args: {
		data: {
			...modalData,
			options: modalData.options.slice(0, 1),
		},
		value: modalValue,
	},
};
